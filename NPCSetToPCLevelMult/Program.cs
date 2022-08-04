using Mutagen.Bethesda;
using Mutagen.Bethesda.FormKeys.SkyrimLE;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NPCSetToPCLevelMult
{
    public class Program
    {
        static Lazy<Settings> Settings = null!;
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetAutogeneratedSettings(
                    nickname: "Settings",
                    path: "settings.json",
                    out Settings)
                .SetTypicalOpen(GameRelease.SkyrimLE, "NPCSetToPCLevelMult.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var ignoreEqualsList = Settings.Value.IgnoreEDIDEquals;
            bool useIgnoreEqualsList = ignoreEqualsList.Count > 0;

            var ignoreStartsWithList = Settings.Value.IgnoreEDIDStartsWith;
            bool useIgnoreStartsWithList = ignoreStartsWithList.Count > 0;

            var ignoreEndsWithList = Settings.Value.IgnoreEDIDStartsWith;
            bool useIgnoreEndsWithList = ignoreEndsWithList.Count > 0;

            var ignoreContainsList = Settings.Value.IgnoreEDIDStartsWith;
            bool useIgnoreContainsList = ignoreContainsList.Count > 0;

            float minMultiplier = Settings.Value.MinLevelMultiplier > 0 ? Settings.Value.MinLevelMultiplier : 0.1F; // hardcoded min is 0.1
            float maxMultiplier = Settings.Value.MaxLevelMultiplier > 0 ? Settings.Value.MaxLevelMultiplier : 1.2F; // hardcoded max is 1.2
            bool set1ForUnique = Settings.Value.Mult1IfUnique != 0.0F;
            bool set1ForEssential = Settings.Value.Mult1IfEssential != 0.0F;
            bool modByWords = Settings.Value.EDIDWordsMultiplierMods.Count > 0.0F;
            bool modStaticByWords = Settings.Value.EDIDWordsStaticMultiplierMods.Count > 0.0F;
            bool modByHeight = Settings.Value.MultiplierModByHeight != 0.0F;
            bool mod4Cowardly = Settings.Value.MultiplierModForCowardly != 0.0F;
            bool mod4Brave = Settings.Value.MultiplierModForBrave != 0.0F;
            bool mod4Foolhardy = Settings.Value.MultiplierModForFoolhardy != 0.0F;

            bool useCustomLevelsSetup = Settings.Value.MultByLevelPairs.Count > 0;
            var multByLevelPairByLevelAscending = useCustomLevelsSetup ? from entry in Settings.Value.MultByLevelPairs orderby entry.MaxLevel ascending select entry : null;

            bool isPlayer = false;

            foreach (var npcGetter in state.LoadOrder.PriorityOrder.Npc().WinningOverrides())
            {
                if (npcGetter == null) continue;

                try
                {
                    // ignore some records by edid
                    var edid = npcGetter.EditorID + "";
                    if (
                        (!isPlayer && (isPlayer = edid == "Player"))
                        ||
                        edid.Contains("AudioTemplate")
                        ||
                        edid.Contains("VoiceType")
                        ||
                        edid.Contains("alePreset")
                        ||
                        edid.ToLowerInvariant().Contains("dummy")
                        )
                    {
                        continue;
                    }

                    // ignore by ignore lists
                    if (useIgnoreEqualsList && ignoreEqualsList.Any(s => edid.ToUpperInvariant() == s.ToUpperInvariant())) continue;
                    if (useIgnoreStartsWithList && ignoreStartsWithList.Any(s => edid.StartsWith(s, StringComparison.OrdinalIgnoreCase))) continue;
                    if (useIgnoreEndsWithList && ignoreEndsWithList.Any(s => edid.EndsWith(s, StringComparison.OrdinalIgnoreCase))) continue;
                    if (useIgnoreContainsList && ignoreContainsList.Any(s => edid.Contains(s, StringComparison.OrdinalIgnoreCase))) continue;
                    //-------------------------

                    bool logMe = Settings.Value.IsDebug;
                    bool recalculateLevelMult = true;
                    var pcLevelMult = npcGetter.Configuration.Level as PcLevelMult;
                    bool isPcLevelMult = pcLevelMult != null;

                    if (logMe) Console.WriteLine("isPcLevelMult=" + isPcLevelMult);
                    if (isPcLevelMult && !recalculateLevelMult && npcGetter.Configuration.CalcMaxLevel == 0) continue;

                    bool isEssential = set1ForEssential && npcGetter.Configuration.Flags.HasFlag(NpcConfiguration.Flag.Essential);
                    bool isUnique = set1ForUnique && npcGetter.Configuration.Flags.HasFlag(NpcConfiguration.Flag.Unique);

                    Npc? npc;
                    var npcConfiguration = npcGetter.Configuration;
                    if (isPcLevelMult && (isUnique || isEssential) && pcLevelMult!.LevelMult == 1.0)
                    {
                        npc = state.PatchMod.Npcs.GetOrAddAsOverride(npcGetter);
                        npc.Configuration.CalcMaxLevel = 0; // just set max level for unique npc else calculate new
                        continue;
                    }

                    // skip when has template
                    var template = npcGetter.Template;
                    if (template != null && !template.IsNull && npcConfiguration.TemplateFlags.HasFlag(NpcConfiguration.TemplateFlag.Stats)) continue;

                    short npcLevel;
                    float oldLevelMult = 1;
                    if (npcGetter.Configuration.Level is NpcLevel npcLvl)
                    {
                        npcLevel = npcLvl.Level;
                    }
                    else
                    {
                        if (isPcLevelMult)
                        {
                            npcLevel = npcConfiguration.CalcMinLevel;
                            oldLevelMult = pcLevelMult!.LevelMult;
                        }
                        else continue;
                    }

                    bool changed = false;

                    if (logMe)
                    {
                        Console.WriteLine("NPC." + edid + ":"
                            + "\r\nMinLevel:" + npcConfiguration.CalcMinLevel
                            + "\r\nMaxLevel:" + npcConfiguration.CalcMaxLevel
                            + "\r\nMult:" + (isPcLevelMult ? pcLevelMult?.LevelMult : "No level mult"));
                    }
                    PcLevelMult npcPcLevelMultData = new();
                    float[] preChangeData = new float[3] { oldLevelMult, npcConfiguration.CalcMinLevel, npcConfiguration.CalcMaxLevel };

                    float npcPcLevelMultDataLevelMult = oldLevelMult;
                    bool skipMultCalculate = isPcLevelMult && npcConfiguration.CalcMinLevel == 0 && npcConfiguration.CalcMaxLevel == 0; // skip records where min and max level equal 0
                    if (!skipMultCalculate && modStaticByWords)
                    {
                        foreach (var wordValue in Settings.Value.EDIDWordsStaticMultiplierMods)
                        {
                            if (!edid.Contains(wordValue.KeyWord, StringComparison.OrdinalIgnoreCase)) continue;

                            npcPcLevelMultDataLevelMult = wordValue.LevelMultiplier;
                            changed = true;
                            skipMultCalculate = true;
                            if (logMe) Console.WriteLine("Mult by static word:" + npcPcLevelMultDataLevelMult);
                            break;
                        }
                    }

                    // player potential follower or marriable has 1.0 mult
                    if (!skipMultCalculate && npcGetter.Factions.Count > 0 && npcGetter.Factions.Any(f => f.Faction == Skyrim.Faction.PotentialFollowerFaction || f.Faction == Skyrim.Faction.CurrentFollowerFaction || f.Faction == Skyrim.Faction.PlayerFaction || f.Faction == Skyrim.Faction.PotentialMarriageFaction))
                    {
                        if (logMe) Console.WriteLine("mult set by faction to 1.0");

                        npcPcLevelMultDataLevelMult = 1.0F;
                        skipMultCalculate = true;
                        changed = true;
                    }

                    // set result level multiplier
                    if (isEssential || isUnique || skipMultCalculate)
                    {
                        if (skipMultCalculate)
                        {
                            if (logMe) Console.WriteLine("skipMultCalculate: mult is " + npcPcLevelMultDataLevelMult);
                        }
                        else if (isUnique)
                        {
                            if (logMe) Console.WriteLine("isUnique: Mult set to" + Settings.Value.Mult1IfUnique);
                            npcPcLevelMultDataLevelMult = Settings.Value.Mult1IfUnique;
                            changed = true;
                        }
                        else if (isEssential)
                        {
                            if (logMe) Console.WriteLine("isEssential: Mult set to" + Settings.Value.Mult1IfEssential);
                            npcPcLevelMultDataLevelMult = Settings.Value.Mult1IfEssential;
                            changed = true;
                        }
                    }
                    else
                    {
                        if (useCustomLevelsSetup)
                        {
                            foreach (var pair in multByLevelPairByLevelAscending!)
                            {
                                if (npcLevel >= pair.MaxLevel) continue;

                                npcPcLevelMultDataLevelMult = pair.LevelMultiplier;
                                changed = true;

                                if (logMe) Console.WriteLine("Mult by level max:" + npcPcLevelMultDataLevelMult);
                                break;
                            }
                        }
                        else
                        {
                            if (npcLevel < 3)
                            {
                                npcPcLevelMultDataLevelMult = 0.1F;
                                changed = true;
                                if (logMe) Console.WriteLine("Mult by level <3:" + npcPcLevelMultDataLevelMult);
                            }
                            else if (npcLevel < 5)
                            {
                                npcPcLevelMultDataLevelMult = 0.2F;
                                changed = true;
                                if (logMe) Console.WriteLine("Mult by level <5:" + npcPcLevelMultDataLevelMult);
                            }
                            else if (npcLevel < 7)
                            {
                                npcPcLevelMultDataLevelMult = 0.5F;
                                changed = true;
                                if (logMe) Console.WriteLine("Mult by level <7:" + npcPcLevelMultDataLevelMult);
                            }
                            else if (npcLevel < 15)
                            {
                                npcPcLevelMultDataLevelMult = 0.8F;
                                changed = true;
                                if (logMe) Console.WriteLine("Mult by level <15:" + npcPcLevelMultDataLevelMult);
                            }
                            else if (npcLevel < 25)
                            {
                                npcPcLevelMultDataLevelMult = 0.9F;
                                changed = true;
                                if (logMe) Console.WriteLine("Mult by level <25:" + npcPcLevelMultDataLevelMult);
                            }
                            else if (npcLevel < 31)
                            {
                                npcPcLevelMultDataLevelMult = 1.0F;
                                changed = true;
                                if (logMe) Console.WriteLine("Mult by level <31:" + npcPcLevelMultDataLevelMult);
                            }
                            else if (npcLevel < 41)
                            {
                                npcPcLevelMultDataLevelMult = 1.1F;
                                changed = true;
                                if (logMe) Console.WriteLine("Mult by level <41:" + npcPcLevelMultDataLevelMult);
                            }
                            else
                            {
                                npcPcLevelMultDataLevelMult = 1.2F;
                                changed = true;
                                if (logMe) Console.WriteLine("Mult by level >40:" + npcPcLevelMultDataLevelMult);
                            }
                        }
                    }


                    if (isEssential || isUnique || skipMultCalculate)
                    {
                        if (logMe) Console.WriteLine("skip mult calc: isEssential=" + isEssential + ",isUnique=" + isUnique + ",skipMultCalculate=" + skipMultCalculate);
                    }
                    else
                    {
                        if (modByWords)
                        {
                            foreach (var wordValue in Settings.Value.EDIDWordsMultiplierMods)
                            {
                                if (!edid.Contains(wordValue.KeyWord, StringComparison.OrdinalIgnoreCase)) continue;
                                npcPcLevelMultDataLevelMult += wordValue.LevelMultiplier;
                                changed = true;
                                if (logMe) Console.WriteLine("Mult by words:" + npcPcLevelMultDataLevelMult);
                                break;
                            }
                        }

                        if (modByHeight)
                        {
                            if (npcGetter.Height < 0.8)
                            {
                                npcPcLevelMultDataLevelMult -= Settings.Value.MultiplierModByHeight;
                                changed = true;
                            }
                            else if (npcGetter.Height > 1.2)
                            {
                                npcPcLevelMultDataLevelMult += Settings.Value.MultiplierModByHeight;
                                changed = true;
                            }
                            if (logMe) Console.WriteLine("Mult after Height check:" + npcPcLevelMultDataLevelMult);
                        }


                        if (!npcConfiguration.TemplateFlags.HasFlag(NpcConfiguration.TemplateFlag.AIData))
                        {
                            if (mod4Cowardly && npcGetter.AIData.Confidence.HasFlag(Confidence.Cowardly))
                            {
                                npcPcLevelMultDataLevelMult += Settings.Value.MultiplierModForCowardly;
                                changed = true;
                            }
                            else if (mod4Brave && npcGetter.AIData.Confidence.HasFlag(Confidence.Brave))
                            {
                                npcPcLevelMultDataLevelMult += Settings.Value.MultiplierModForBrave;
                                changed = true;
                            }
                            else if (mod4Foolhardy && npcGetter.AIData.Confidence.HasFlag(Confidence.Foolhardy))
                            {
                                npcPcLevelMultDataLevelMult += Settings.Value.MultiplierModForFoolhardy;
                                changed = true;
                            }

                            if (logMe) Console.WriteLine("Mult after Confidence check:" + npcPcLevelMultDataLevelMult);
                        }
                    }

                    if (isPcLevelMult && changed && npcPcLevelMultDataLevelMult < preChangeData[0])
                    {
                        if (logMe) Console.WriteLine("Mult " + npcPcLevelMultDataLevelMult + " set to prechanged:" + preChangeData[0]);
                        npcPcLevelMultDataLevelMult = preChangeData[0];
                    }

                    if (npcPcLevelMultDataLevelMult < minMultiplier)
                    {
                        if (logMe) Console.WriteLine("Mult is" + npcPcLevelMultDataLevelMult + ", below min " + minMultiplier + ", set to min");
                        npcPcLevelMultData.LevelMult = minMultiplier;
                    }
                    else if (npcPcLevelMultDataLevelMult > maxMultiplier)
                    {
                        if (logMe) Console.WriteLine("Mult is" + npcPcLevelMultDataLevelMult + ", above max " + maxMultiplier + ", set to max");
                        npcPcLevelMultData.LevelMult = maxMultiplier;
                    }
                    else
                    {
                        if (logMe) Console.WriteLine("Mult set to " + npcPcLevelMultDataLevelMult);
                        npcPcLevelMultData.LevelMult = npcPcLevelMultDataLevelMult;
                    }

                    bool notIsPcLevelMult = !isPcLevelMult;
                    bool preNpcPcLevelMultDataLevelMult = preChangeData[0] != npcPcLevelMultDataLevelMult;
                    bool preNpcLevel = preChangeData[1] != npcLevel;
                    bool preMaxLevelCalc = preChangeData[2] != Settings.Value.MaxLevelCalc;

                    if (logMe) Console.WriteLine($"{nameof(notIsPcLevelMult)}={notIsPcLevelMult},{nameof(preNpcPcLevelMultDataLevelMult)}={preNpcPcLevelMultDataLevelMult},{nameof(preNpcLevel)}={preNpcLevel},{nameof(preMaxLevelCalc)}={preMaxLevelCalc},{nameof(changed)}={changed}");

                    if (notIsPcLevelMult || (changed && (preNpcPcLevelMultDataLevelMult || preNpcLevel || preMaxLevelCalc))) // patch only if mult or min level changed
                    {
                        if (logMe) Console.WriteLine("Result mult:" + npcPcLevelMultData.LevelMult + "\r\n");

                        // patch record
                        npc = state.PatchMod.Npcs.GetOrAddAsOverride(npcGetter);
                        npc.Configuration.Level = npcPcLevelMultData;
                        npc.Configuration.CalcMinLevel = npcLevel;
                        npc.Configuration.CalcMaxLevel = Settings.Value.MaxLevelCalc;
                    }
                    else if (logMe) Console.WriteLine("Result mult is not applied:" + npcPcLevelMultDataLevelMult + "\r\n");
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine("An ArgumentException error accured while parse npc '" + npcGetter.FormKey.ID + "'(" + npcGetter.EditorID + ":" + npcGetter.Name + ") Error:\r\n" + ex + "\r\n");
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine("An NullReferenceException error accured while parse npc '" + npcGetter.FormKey.ID + "'(" + npcGetter.EditorID + ":" + npcGetter.Name + ") Error:\r\n" + ex + "\r\n");
                }
            }
        }
    }
}

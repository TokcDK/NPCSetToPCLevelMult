using Mutagen.Bethesda.Synthesis.Settings;
using Mutagen.Bethesda.WPF.Reflection.Attributes;
using System.Collections.Generic;

namespace NPCSetToPCLevelMult
{
    public class MultByLevelPairsData
    {
        [SynthesisOrder]
        [SynthesisSettingName("Maximum NPC level")]
        [SynthesisTooltip("Maxinmum NPC level before which will be set Level multiplier")]
        public int MaxLevel;

        [SynthesisOrder]
        [SynthesisSettingName("NPC level multiplier")]
        [SynthesisTooltip("NPC level multiplier which must be set for all npc level of which lesser of Max level")]
        public float LevelMultiplier;
    }
    public class KeyWordData
    {
        [SynthesisOrder]
        [SynthesisSettingName("Keyword string")]
        [SynthesisTooltip("Part of npc Editor ID keyword string which can be in npc's editor id. See parent desxription.")]
        public string KeyWord = "partofeditorid";

        [SynthesisOrder]
        [SynthesisSettingName("NPC level multiplier")]
        [SynthesisTooltip("NPC level multiplier for the keyword. See parent desxription.")]
        public float LevelMultiplier = 1.0F;
    }

    public class Settings
    {
        [SynthesisOrder]
        [SynthesisSettingName("Max calculated level")]
        [SynthesisTooltip("Maximal calculated level value limit. 0 by default = no limit.")]
        public short MaxLevelCalc = 0;

        [SynthesisOrder]
        [SynthesisSettingName("Min allowed level mult")]
        [SynthesisTooltip("Minimal allowed value of level multiplier after all calculations. Any lover of it will be reverted to it. Cant be lower of 0.1")]
        public float MinLevelMultiplier = 0.1F;

        [SynthesisOrder]
        [SynthesisSettingName("Max allowed level mult")]
        [SynthesisTooltip("Maximal allowed value of level multiplier after all calculations. Any higher of it will be reverted to it.")]
        public float MaxLevelMultiplier = 1.2F;

        [SynthesisOrder]
        [SynthesisSettingName("Force set unique npc mult")]
        [SynthesisTooltip("Force set Unique npc level multiplier. 0 = not set")]
        public float Mult1IfUnique = 1.0F;

        [SynthesisOrder]
        [SynthesisSettingName("Force set essential npc mult")]
        [SynthesisTooltip("Force set Essential npc level multiplier. 0 = not set")]
        public float Mult1IfEssential = 1.0F;

        [SynthesisOrder]
        [SynthesisSettingName("Mult assigment by <max level")]
        [SynthesisTooltip("Set it from smaller to bigger. Default: <3=0.1,<5=0.2,<7=0.5,<15=0.8,<25=0.9,<31=1.0,<41=1.1,>40=1.2")]
        public HashSet<MultByLevelPairsData> MultByLevelPairs = new();

        [SynthesisOrder]
        [SynthesisSettingName("Static multiplier for npc by keyword")]
        [SynthesisTooltip("Static multiplier for npc that in their editor id have this keyword string. Example: keyword=alduin,mult=1.2")]
        public HashSet<KeyWordData> EDIDWordsStaticMultiplierMods = new();

        [SynthesisOrder]
        [SynthesisSettingName("Milt mod by keyword string")]
        [SynthesisTooltip("When NPC's Editor ID contains entered Keyword, his result multiplier will be + entered level mult. Example 1: keyword=boss,mult=0.3,result 0.8+0.3=1.1 / Example 2: keyword=rabbit,mult=0.3,result 0.8+(-0.3)=0.5")]
        public HashSet<KeyWordData> EDIDWordsMultiplierMods = new();

        [SynthesisOrder]
        [SynthesisSettingName("Milt mod for Cowardly")]
        [SynthesisTooltip("Will reduce multiplier by selected value if selected npc is Cowardly. Example: result 0.5 + default -0.1 = 0.4")]
        public float MultiplierModForCowardly = -0.1F;
        
        [SynthesisOrder]
        [SynthesisSettingName("Milt mod for Brave")]
        [SynthesisTooltip("Will increase multiplier if npc is Brave. Example: result 0.5 + default 0.1 = 0.6")]
        public float MultiplierModForBrave = 0.1F;

        [SynthesisOrder]
        [SynthesisSettingName("Milt mod for Foolhardy")]
        [SynthesisTooltip("Will increase multiplier if npc is Foolhardy. Example: result 0.5 + default 0.1 = 0.6")]
        public float MultiplierModForFoolhardy = 0.1F;

        [SynthesisOrder]
        [SynthesisSettingName("Milt mod by height 0.8-/1.2+")]
        [SynthesisTooltip("Will mod multiplier by selected value if height of selected npc lower of 0.8/higher of 1.2")]
        public float MultiplierModByHeight = 0.1F;

        [SynthesisOrder]
        [SynthesisSettingName("Ignore editorid list")]
        [SynthesisTooltip("Ignore all records which editorid is equals to any of this string keywords")]
        public HashSet<string> IgnoreEDIDEquals = new();

        [SynthesisOrder]
        [SynthesisSettingName("Ignore startswith keywords")]
        [SynthesisTooltip("Ignore all records which editorid starts with any of this strings")]
        public HashSet<string> IgnoreEDIDStartsWith = new();

        [SynthesisOrder]
        [SynthesisSettingName("Ignore endswith keywords")]
        [SynthesisTooltip("Ignore all records which editor id ends with any of this strings")]
        public HashSet<string> IgnoreEDIDEndsWith = new();
       
        [SynthesisOrder]
        [SynthesisSettingName("Ignore contains keywords")]
        [SynthesisTooltip("Ignore all records which editor id contains any of this strings")]
        public HashSet<string> IgnoreEDIDContains = new();
        
        [SynthesisOrder]
        [SynthesisSettingName("Debug mode")]
        [SynthesisTooltip("Additional messages, dont need for normal use")]
        public bool IsDebug = false;
    }
}

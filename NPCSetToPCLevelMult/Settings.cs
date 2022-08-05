using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis.Settings;
using StringCompareSettings;
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
        [SynthesisTooltip("Part of npc Editor ID keyword string which can be in npc's editor id. See parent description.")]
        public string KeyWord = "partofeditorid";

        [SynthesisOrder]
        [SynthesisSettingName("NPC level multiplier")]
        [SynthesisTooltip("NPC level multiplier for the keyword. See parent description.")]
        public float LevelMultiplier = 1.0F;

        [SynthesisOrder]
        [SynthesisTooltip("Comment for the record")]
        public string Comment = "";
    }

    public class LevelMultByFormkeyData
    {
        [SynthesisOrder]
        [SynthesisSettingName("Target FormKey")]
        [SynthesisTooltip("Formkey for which set multiplier. See parent description.")]
        public FormKey FormKey;

        [SynthesisOrder]
        [SynthesisSettingName("NPC level multiplier")]
        [SynthesisTooltip("NPC level multiplier for the Formkey. See parent description.")]
        public float LevelMultiplier;

        [SynthesisOrder]
        //[SynthesisSettingName("NPC level multiplier")]
        [SynthesisTooltip("Comment for the record")]
        public string Comment = "";
    }

    public class LevelMultByClassData
    {
        [SynthesisOrder]
        [SynthesisSettingName("Target class")]
        [SynthesisTooltip("Class for which set multiplier. See parent description.")]
        public IFormLinkGetter<IClassGetter>? Class;

        [SynthesisOrder]
        [SynthesisSettingName("NPC level multiplier")]
        [SynthesisTooltip("NPC level multiplier for the race. See parent description.")]
        public float LevelMultiplier;

        [SynthesisOrder]
        [SynthesisTooltip("Comment for the record")]
        public string Comment = "";
    }

    public class LevelMultByRaceData
    {
        [SynthesisOrder]
        [SynthesisSettingName("Target race")]
        [SynthesisTooltip("Race for which set multiplier. See parent description.")]
        public IFormLinkGetter<IRaceGetter>? Race;

        [SynthesisOrder]
        [SynthesisSettingName("NPC level multiplier")]
        [SynthesisTooltip("NPC level multiplier for the race. See parent description.")]
        public float LevelMultiplier;

        [SynthesisOrder]
        [SynthesisTooltip("Comment for the record")]
        public string Comment = "";
    }

    public class Settings
    {
        [SynthesisOrder]
        [SynthesisSettingName("Mouse over will show parameter description")]
        [SynthesisTooltip("This is note for users to know how to see parameter description")]
        public bool Note = false;

        [SynthesisOrder]
        [SynthesisSettingName("Max calculated level")]
        [SynthesisTooltip("Maximal calculated level value limit. 0 by default = no limit.")]
        public short MaxLevelCalc = 0;

        [SynthesisOrder]
        [SynthesisSettingName("Min allowed level mult")]
        [SynthesisTooltip("Minimal allowed value of level multiplier after all calculations. Any lower of it will be reverted to it. Cant be lower of 0.1")]
        public float MinLevelMultiplier = 0.1F;

        [SynthesisOrder]
        [SynthesisSettingName("Max allowed level mult")]
        [SynthesisTooltip("Maximal allowed value of level multiplier after all calculations. Any higher of it will be reverted to it.")]
        public float MaxLevelMultiplier = 1.2F;

        [SynthesisOrder]
        //[SynthesisSettingName("Max allowed level mult for npc class")]
        [SynthesisTooltip($"Priority higher of {nameof(MaxLevelMultiplier)}. Maximum allowed value of level multiplier after all calculations is npc has the class. Any higher of it will be reverted to it. Cant be lower of 0.1")]
        public HashSet<LevelMultByClassData> MaxLvMultClass = new()
        {
            new LevelMultByClassData(){ Class=new FormLink<IClassGetter>(FormKey.Factory("01326B:Skyrim.esm")), LevelMultiplier=0.7F, Comment="Citizen class is weaker" }, // Citizen class
        };

        [SynthesisOrder]
        //[SynthesisSettingName("Max allowed level mult for npc race")]
        [SynthesisTooltip($"Priority higher of {nameof(MaxLevelMultiplier)} && {nameof(MaxLvMultClass)}. Maximum allowed value of level multiplier after all calculations is npc has the race. Any higher of it will be reverted to it. Cant be lower of 0.1")]
        public HashSet<LevelMultByRaceData> MaxLvMultRace = new()
        {
            new LevelMultByRaceData(){ Race=new FormLink<IRaceGetter>(FormKey.Factory("067CD8:Skyrim.esm")), LevelMultiplier=0.5F, Comment="Elder race is weaker" }, // Elder race
        };

        [SynthesisOrder]
        [SynthesisSettingName("Force set unique npc mult")]
        [SynthesisTooltip("Force set Unique npc level multiplier. 0 = not set")]
        public float StaticMult4Unique = 1.0F;

        [SynthesisOrder]
        [SynthesisSettingName("Force set essential npc mult")]
        [SynthesisTooltip("Force set Essential npc level multiplier. 0 = not set")]
        public float StaticMult4Essential = 1.0F;

        [SynthesisOrder]
        //[SynthesisSettingName("Mult assigment by <max level\n")]
        [SynthesisTooltip("Set it from smaller to bigger. Default: <3=0.1,<5=0.2,<7=0.5,<15=0.8,<25=0.9,<31=1.0,<41=1.1,>40=1.2")]
        public HashSet<MultByLevelPairsData> MultByLevelPairs = new()
        {
            new MultByLevelPairsData(){ MaxLevel=3,LevelMultiplier=0.1F },
            new MultByLevelPairsData(){ MaxLevel=5,LevelMultiplier=0.2F },
            new MultByLevelPairsData(){ MaxLevel=7,LevelMultiplier=0.5F },
            new MultByLevelPairsData(){ MaxLevel=15,LevelMultiplier=0.8F },
            new MultByLevelPairsData(){ MaxLevel=25,LevelMultiplier=0.9F },
            new MultByLevelPairsData(){ MaxLevel=31,LevelMultiplier=1.0F },
            new MultByLevelPairsData(){ MaxLevel=41,LevelMultiplier=1.1F },
            new MultByLevelPairsData(){ MaxLevel=999,LevelMultiplier=1.2F },
        };

        [SynthesisOrder]
        //[SynthesisSettingName("Static multiplier for npc by keyword\n")]
        [SynthesisTooltip("Static multiplier for npc that in their editor id have this keyword string. Example: keyword=alduin,mult=1.2")]
        public HashSet<KeyWordData> StaticMultMods = new()
        {
            new KeyWordData(){ KeyWord="MQ106Alduin", LevelMultiplier=1.2F, Comment="Alduin is alwais stronger" }
        };

        [SynthesisOrder]
        //[SynthesisSettingName("Mult mod by keyword string\n")]
        [SynthesisTooltip("When NPC's Editor ID contains entered Keyword, his result multiplier will be + entered level mult. Example 1: keyword=boss,mult=0.3,result 0.8+0.3=1.1 / Example 2: keyword=rabbit,mult=0.3,result 0.8+(-0.3)=0.5")]
        public HashSet<KeyWordData> MultMods = new()
        {
            new KeyWordData(){ KeyWord="Matriarch",LevelMultiplier=0.3F },
            new KeyWordData(){ KeyWord="Patriarch",LevelMultiplier=0.3F },
            new KeyWordData(){ KeyWord="Boss",LevelMultiplier=0.2F },
            new KeyWordData(){ KeyWord="Leader",LevelMultiplier=0.2F },
            new KeyWordData(){ KeyWord="Lord",LevelMultiplier=0.2F },
            new KeyWordData(){ KeyWord="Chief",LevelMultiplier=0.2F },
            new KeyWordData(){ KeyWord="Dragon",LevelMultiplier=0.2F },
            new KeyWordData(){ KeyWord="Giant",LevelMultiplier=0.2F },
            new KeyWordData(){ KeyWord="Ogre",LevelMultiplier=0.2F },
            new KeyWordData(){ KeyWord="Captain",LevelMultiplier=0.15F },
            new KeyWordData(){ KeyWord="Dread",LevelMultiplier=0.1F },
            new KeyWordData(){ KeyWord="Sergeant",LevelMultiplier=0.1F },
        };

        [SynthesisOrder]
        [SynthesisSettingName("Multiplier mod by npc confidence flag equal. result mult+mod mult")]
        public Dictionary<Confidence, float> ModByConfidence = new()
        {
            {Confidence.Cowardly, -0.1F },
            {Confidence.Brave, 0.1F },
            {Confidence.Foolhardy, 0.1F },
        };

        [SynthesisOrder]
        [SynthesisSettingName("Mult mod by height 0.8-/1.2+")]
        [SynthesisTooltip("Will change multiplier by selected value if height of selected npc lower of 0.8/higher of 1.2")]
        public float MultModByHeight = 0.1F;

        [SynthesisTooltip("List of string keywords to skip npc by their editor id")]
        public HashSet<StringCompareSettingContainer> IgnoreList = new()
        {
            new StringCompareSettingContainer(){ StringSetting=new StringCompareSetting(){ Name="audiotemplate" } },
            new StringCompareSettingContainer(){ StringSetting=new StringCompareSetting(){ Name="voicetype" } },
            new StringCompareSettingContainer(){ StringSetting=new StringCompareSetting(){ Name="corpse", Comment="dont touch corpse npc" } },
            new StringCompareSettingContainer(){ StringSetting=new StringCompareSetting(){ Name="dummy", Comment="some dummy npc" } },
            new StringCompareSettingContainer(){ StringSetting=new StringCompareSetting(){ Name="preset", Comment="some preset npc" } },
            new StringCompareSettingContainer(){ StringSetting=new StringCompareSetting(){ Name="cb2_", Compare=CompareType.StartsWith, Comment="dummy npc from 'Collector bags 2' mod" } },
            new StringCompareSettingContainer(){ StringSetting=new StringCompareSetting(){ Name="fstest", Compare=CompareType.StartsWith, Comment="some test npc" } },
        };

        [SynthesisOrder]
        [SynthesisSettingName("Debug mode")]
        [SynthesisTooltip("Additional messages, dont need for normal use")]
        public bool IsDebug = false;
    }
}

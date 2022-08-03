using Mutagen.Bethesda.WPF.Reflection.Attributes;
using System.Collections.Generic;

namespace NPCSetToPCLevelMult
{
    internal class Settings
    {
        [SettingName("Maximal calculated level value limit. 0 by default = no limit.")]
        public short MaxLevelCalc = 0;
        [SettingName("Minimal allowed value of level multiplier after all calculations")]
        public float MinimalMultiplier = 0.1F;
        [SettingName("Maximal allowed value of level multiplier after all calculations")]
        public float MaximalMultiplier = 1.2F;
        [SettingName("Always set multiplier to 1.0 for unique npc")]
        public float Mult1IfUnique = 1.0F;
        [SettingName("Always set multiplier to 1.0 for essential npc")]
        public float Mult1IfEssential = 1.0F;
        [SettingName("Level Assigment pairs. Set it from smaller to bigger. Default:<3=0.1,<5=0.2,<7=0.5,<15=0.8,<25=0.9,<31=1.0,<41=1.1,>40=1.2")]
        public Dictionary<short, float> MultByLevelPairs = new();
        [SettingName("Static multiplier for npc that in thier editor id have this words. Enter word/static multiplier value")]
        public Dictionary<string, float> EDIDWordsStaticMultiplierMods = new();
        [SettingName("If any set it will modify multiplier by entered value if npc editorid contains entered word. Enter word/multiplier modifier value")]
        public Dictionary<string, float> EDIDWordsMultiplierMods = new();
        [SettingName("Will reduce multiplier by selected value if selected npc is cowardly and will raise if brave or foolhardy")]
        public float MultiplierModByConfidence = 0.1F;
        [SettingName("Will reduce or raise multiplier by selected value if height of selected npc lower of 0.8/higher of 1.2")]
        public float MultiplierModByHeight = 0.1F;

        [SettingName("Ignore all records which editor id equals to any of this strings")]
        public List<string> IgnoreEDIDEquals = new();

        [SettingName("Ignore all records which editor id starts with any of this strings")]
        public List<string> IgnoreEDIDStartsWith = new();

        [SettingName("Ignore all records which editor id ends with any of this strings")]
        public List<string> IgnoreEDIDEndsWith = new();

        [SettingName("Ignore all records which editor id contains any of this strings")]
        public List<string> IgnoreEDIDContains = new();
    }
}

using Mutagen.Bethesda.Synthesis.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StringCompareSettings
{
    public class StringCompareSetting
    {
        [SynthesisTooltip("String keyword name which search for")]
        public string? Name;
        [SynthesisSettingName("Compare type")]
        [SynthesisTooltip("Compare type, how to compare")]
        public CompareType Compare = CompareType.Contains;
        [SynthesisTooltip("Case insensetive compare, comparing ignore case")]
        public bool IgnoreCase = true;
        [SynthesisTooltip("Commentary for the strings. Just to understand")]
        public string? Comment;
    }

    public class StringCompareSettingContainer
    {
        [SynthesisSettingName("String")]
        [SynthesisTooltip("Click to open string parameters")]
        public StringCompareSetting? StringSetting;
    }

    public enum CompareType
    {
        Equals,
        StartsWith,
        Contains,
        EndsWith,
        Regex,
    }

    public static class StringCompareHelpers
    {
        public static bool IsUsingSkipList = false;

        public static bool IsInSkipList(this string? inputString, HashSet<StringCompareSettingContainer> list)
        {
            if (IsUsingSkipList) return false;
            if (string.IsNullOrWhiteSpace(inputString)) return false;

            foreach (var setting in list)
            {
                if (setting.StringSetting==null) continue;

                var s = setting.StringSetting;
                if (string.IsNullOrWhiteSpace(s.Name)) continue;

                if (s.Compare == CompareType.Contains)
                {
                    if (s.IgnoreCase)
                    {
                        if (inputString.Contains(s.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (inputString.Contains(s.Name, StringComparison.InvariantCulture))
                        {
                            return true;
                        }
                    }
                }
                else if (s.Compare == CompareType.Equals)
                {
                    if (s.IgnoreCase)
                    {
                        if (string.Equals(inputString, s.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (string.Equals(inputString, s.Name, StringComparison.InvariantCulture))
                        {
                            return true;
                        }
                    }
                }
                else if (s.Compare == CompareType.StartsWith)
                {
                    if (s.IgnoreCase)
                    {
                        if (inputString.StartsWith(s.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (inputString.StartsWith(s.Name, StringComparison.InvariantCulture))
                        {
                            return true;
                        }
                    }
                }
                else if (s.Compare == CompareType.EndsWith)
                {
                    if (s.IgnoreCase)
                    {
                        if (inputString.EndsWith(s.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (inputString.EndsWith(s.Name, StringComparison.InvariantCulture))
                        {
                            return true;
                        }
                    }
                }
                else if (s.Compare == CompareType.Regex)
                {
                    try
                    {
                        if (s.IgnoreCase)
                        {
                            if (Regex.IsMatch(inputString, s.Name, RegexOptions.IgnoreCase))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (Regex.IsMatch(inputString, s.Name, RegexOptions.None))
                            {
                                return true;
                            }
                        }
                    }
                    catch { }
                }
            }

            return true;
        }
    }
}

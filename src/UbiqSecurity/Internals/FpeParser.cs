using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UbiqSecurity.Model;

namespace UbiqSecurity.Internals
{
    internal static class FpeParser
    {
        public static FpeParseModel Parse(string input, FfsRecord dataset, bool encrypt)
        {
            string firstCharacter;
            string validChars;

            if (encrypt)
            {
                validChars = dataset.InputCharacters;
                firstCharacter = dataset.OutputCharacters.Substring(0, 1);
            }
            else
            {
                validChars = dataset.OutputCharacters;
                firstCharacter = dataset.InputCharacters.Substring(0, 1);
            }

            var trimmedSb = new StringBuilder();
            var templateSb = new StringBuilder();

            var result = new FpeParseModel()
            {
                PassthroughProcessed = false,
                TemplateChar = firstCharacter.First()
            };

            // backwards compatibility for old datasets that don't support Dataset.PassthroughRules and still uses Dataset.PassthroughCharacters
            if (!string.IsNullOrEmpty(dataset.PassthroughCharacters) && (dataset.PassthroughRules == null || !dataset.PassthroughRules.Any()))
            {
                dataset.PassthroughRules = new List<PassthroughRuleDto>()
                {
                    new PassthroughRuleDto()
                    {
                        Priority = 1,
                        Value = dataset.PassthroughCharacters,
                        RuleType = PassthroughRuleType.Passthrough
                    }
                };
            }

            var passthroughCharacters = dataset.PassthroughCharacters;

            foreach (var rule in dataset.PassthroughRules.OrderBy(x => x.Priority))
            {
                switch (rule.RuleType)
                {
                    case PassthroughRuleType.Passthrough:
                        ProcessPassthrough(input, result, passthroughCharacters);
                        break;

                    case PassthroughRuleType.Prefix:
                        var prefixLength = int.Parse(rule.Value, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        input = ProcessPrefix(input, result, prefixLength, passthroughCharacters);
                        break;

                    case PassthroughRuleType.Suffix:
                        var suffixLength = int.Parse(rule.Value, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        input = ProcessSuffix(input, result, suffixLength, passthroughCharacters);
                        break;

                    case PassthroughRuleType.None:
                    default:
                        break;
                }
            }

            ValidateFpeParseModel(result, validChars);

            return result;
        }

        public static string ProcessPassthrough(string input, FpeParseModel parseModel, string passthroughCharacters)
        {
            var templateBuilder = new StringBuilder();
            var trimmedBuilder = new StringBuilder();

            foreach (var c in input.ToCharArray())
            {
                if (passthroughCharacters.IndexOf(c) != -1)
                {
                    templateBuilder.Append(c);
                }
                else
                {
                    trimmedBuilder.Append(c);
                    templateBuilder.Append(parseModel.TemplateChar);
                }
            }

            parseModel.StringTemplate = templateBuilder.ToString();
            parseModel.Trimmed = trimmedBuilder.ToString();
            parseModel.PassthroughProcessed = true;

            return input;
        }

        public static string ProcessPrefix(string input, FpeParseModel parseModel, int prefixLength, string passthroughCharacters)
        {
            if (!parseModel.PassthroughProcessed)
            {
                parseModel.Prefix = input.Substring(0, prefixLength);
                return input.Substring(prefixLength);
            }

            var prefixBuilder = new StringBuilder();
            int i = 0;
            while (i < prefixLength)
            {
                char ch = parseModel.StringTemplate.First();

                if (passthroughCharacters.IndexOf(ch) != -1)
                {
                    prefixBuilder.Append(ch);
                }
                else
                {
                    prefixBuilder.Append(parseModel.Trimmed.First());
                    parseModel.Trimmed = parseModel.Trimmed.TrimStart(1);
                    i++;
                }

                parseModel.StringTemplate = parseModel.StringTemplate.TrimStart(1);
            }

            parseModel.Prefix = prefixBuilder.ToString();

            return input;
        }

        public static string ProcessSuffix(string input, FpeParseModel parseModel, int suffixLength, string passthroughCharacters)
        {
            if (!parseModel.PassthroughProcessed)
            {
                parseModel.Suffix = input.Substring(input.Length - suffixLength);
                return input.Substring(0, input.Length - suffixLength);
            }

            var suffixBuilder = new StringBuilder();
            int i = 0;
            while (i < suffixLength)
            {
                char ch = parseModel.StringTemplate.Last();

                if (passthroughCharacters.IndexOf(ch) != -1)
                {
                    suffixBuilder.Insert(0, ch);
                }
                else
                {
                    suffixBuilder.Insert(0, parseModel.Trimmed.Last());
                    parseModel.Trimmed = parseModel.Trimmed.TrimEnd(1);
                    i++;
                }

                parseModel.StringTemplate = parseModel.StringTemplate.TrimEnd(1);
            }

            parseModel.Suffix = suffixBuilder.ToString();

            return input;
        }

        public static void ValidateFpeParseModel(FpeParseModel parseModel, string sourceCharacterSet)
        {
            foreach (var c in parseModel.Trimmed)
            {
                if (sourceCharacterSet.IndexOf(c) == -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(parseModel), $"Trimmed input string has invalid character:  '{c}'");
                }
            }
        }
    }
}

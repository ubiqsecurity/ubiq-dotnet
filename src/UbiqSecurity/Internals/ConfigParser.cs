using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace UbiqSecurity.Internals
{
    internal class ConfigParser
    {
        private readonly IDictionary<string, IDictionary<string, string>> _sections;

        public ConfigParser(string pathName)
        {
            _sections = new Dictionary<string, IDictionary<string, string>>();

            if (string.IsNullOrWhiteSpace(pathName))
            {
                throw new ArgumentException("pathName is empty", nameof(pathName));
            }

            if (!File.Exists(pathName))
            {
                throw new ArgumentException($"file does not exist: {pathName}", nameof(pathName));
            }

            var lines = File.ReadAllLines(pathName);
            ReadLines(pathName, lines);
        }

        internal string GetValue(string section, string key)
        {
            if (_sections.ContainsKey(section))
            {
                var theSection = _sections[section];
                if (theSection.ContainsKey(key))
                {
                    return theSection[key];
                }
            }

            return null;
        }

        private void ReadLines(string pathname, IEnumerable<string> lines)
        {
            int lineNumber = 0;
            IDictionary<string, string> currentSection = null;

            // match '[section]' lines
            var headerRegex = new Regex(@"\[\s*([- \w]+)\s*\]");

            // match 'name = value' lines
            var keyValueRegex = new Regex(@"([^\s=]+)\s*=\s*([^\s=]+)");

            foreach (var line in lines)
            {
                lineNumber++;

                var trimmedLine = line.Trim();

                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    continue;
                }

                if (trimmedLine.StartsWith("[", true, CultureInfo.InvariantCulture)
                    && trimmedLine.EndsWith("]", true, CultureInfo.InvariantCulture))
                {
                    var match = headerRegex.Match(trimmedLine);
                    if (match.Groups.Count == 2)
                    {
                        var newSectionName = match.Groups[1].Value.ToLowerInvariant();

                        if (_sections.ContainsKey(newSectionName))
                        {
                            throw new InvalidDataException($"duplicate section name '{newSectionName}' in file '{pathname}', line {lineNumber}");
                        }

                        // start a new section
                        currentSection = new Dictionary<string, string>();
                        _sections.Add(newSectionName, currentSection);
                    }
                }
                else if ((currentSection != null) && trimmedLine.Contains("="))
                {
                    var match = keyValueRegex.Match(trimmedLine);
                    if (match.Groups.Count == 3)
                    {
                        var key = match.Groups[1].Value.ToLowerInvariant();
                        var value = match.Groups[2].Value;

                        if (currentSection.ContainsKey(key))
                        {
                            throw new InvalidDataException($"duplicate key detected '{key}' in file '{pathname}', line {lineNumber}");
                        }

                        currentSection.Add(key, value);
                    }
                }
            }
        }
    }
}

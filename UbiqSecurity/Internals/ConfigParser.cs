using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace UbiqSecurity.Internals
{
    internal class ConfigParser
    {
        private readonly IDictionary<string, IDictionary<string, string>> _sections;

        #region Constructors
        public ConfigParser(string pathname)
        {
            _sections = new Dictionary<string, IDictionary<string, string>>();

            if (string.IsNullOrWhiteSpace(pathname))
            {
                throw new ArgumentException(nameof(pathname));
            }
            else if (!File.Exists(pathname))
            {
                throw new ArgumentException($"file does not exist: {pathname}", nameof(pathname));
            }

            var lines = File.ReadAllLines(pathname);
            ReadLines(pathname, lines);
        }
        #endregion

        #region Methods
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
        #endregion

        #region Private Methods
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

                if (!string.IsNullOrWhiteSpace(trimmedLine))
                {
                    if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                    {
                        var match = headerRegex.Match(trimmedLine);
                        if (match.Groups.Count == 2)
                        {
                            var newSectionName = match.Groups[1].Value.ToLower();
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
                            var key = match.Groups[1].Value.ToLower();
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
        #endregion
    }
}
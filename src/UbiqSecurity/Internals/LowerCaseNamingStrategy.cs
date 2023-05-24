﻿using Newtonsoft.Json.Serialization;

namespace UbiqSecurity.Internals
{
	internal class LowerCaseNamingStrategy : NamingStrategy
	{
		public LowerCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames)
		{
			ProcessDictionaryKeys = processDictionaryKeys;
			OverrideSpecifiedNames = overrideSpecifiedNames;
		}

		public LowerCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames, bool processExtensionDataNames)
			: this(processDictionaryKeys, overrideSpecifiedNames)
		{
			ProcessExtensionDataNames = processExtensionDataNames;
		}

		public LowerCaseNamingStrategy() { }

		protected override string ResolvePropertyName(string name) => name?.ToLowerInvariant();
	}
}

using System.Reflection;
using System.Text.Json;
using Xunit.Sdk;

namespace UbiqSecurity.Tests.Helpers
{
	// <summary>
	/// Custom xUnit data attribute to pull test data from JSON file
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	internal class JsonTestDataAttribute : DataAttribute
	{
		private readonly IEnumerable<string> _filePaths;

		public JsonTestDataAttribute()
		{
			// default to 100.json but allow automated tests to override that w/ an environment variable
			// if we default to something larger, the VS Test Explorer will take forever to enumerate all possible tests
			// environment variable can be passed on commandline, for example: dotnet test -e JsonTestSize:"10k"
			var folderPath = $@"TestData/{Environment.GetEnvironmentVariable("JsonTestEnv") ?? "prod"}/{Environment.GetEnvironmentVariable("JsonTestSize") ?? "100"}";
            _filePaths = Directory.EnumerateFiles(folderPath, "*.json");
		}

		public override IEnumerable<object[]> GetData(MethodInfo testMethod)
		{
			var methodParameters = testMethod.GetParameters();
			var parameterTypes = methodParameters.Select(x => x.ParameterType).ToArray();

            foreach (var filePath in _filePaths)
            {
                using var fileStream = File.OpenRead(filePath);

                JsonElement json = JsonDocument.Parse(fileStream).RootElement;
                if (json.ValueKind != JsonValueKind.Array)
                {
                    throw new Exception("JSON file does not contain an array");
                }

                foreach (JsonElement element in json.EnumerateArray())
                {
                    var objects = new List<object>();

                    foreach (var parameter in methodParameters)
                    {
                        // find json property w/ same name as the method parameter
                        var property = element.GetProperty(parameter.Name.ToLowerInvariant());

                        object value = parameter.ParameterType switch
                        {
                            Type stringType when stringType == typeof(string) => property.GetString(),
                            Type intType when intType == typeof(int) => property.GetInt32(),
                            Type nullableIntType when nullableIntType == typeof(int?) => string.IsNullOrWhiteSpace(property.GetString()) ? (int?)null : property.GetInt32(),
                            _ => property.GetString()
                        };

                        objects.Add(value);

                    }

                    yield return objects.ToArray();
                }
            }
		}
	}
}

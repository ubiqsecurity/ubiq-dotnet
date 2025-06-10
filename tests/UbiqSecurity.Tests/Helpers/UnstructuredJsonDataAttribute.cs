using System.Reflection;
using System.Text.Json;
using Xunit.Sdk;

namespace UbiqSecurity.Tests.Helpers
{
	// <summary>
	/// Custom xUnit data attribute to pull test data from JSON file
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	internal class UnstructuredJsonTestDataAttribute : DataAttribute
	{
		private readonly string _filePath;

		public UnstructuredJsonTestDataAttribute()
		{
			// default to 100.json but allow automated tests to override that w/ an environment variable
			// if we default to something larger, the VS Test Explorer will take forever to enumerate all possible tests
			// environment variable can be passed on commandline, for example: dotnet test -e JsonTestSize:"10k"
			_filePath = $@"TestData/unstructured-{Environment.GetEnvironmentVariable("JsonTestSize") ?? "100"}.json";
		}

		public override IEnumerable<object[]> GetData(MethodInfo testMethod)
		{
			var methodParameters = testMethod.GetParameters();
			var parameterTypes = methodParameters.Select(x => x.ParameterType).ToArray();

			using var fileStream = File.OpenRead(_filePath);

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

using UbiqSecurity.Internals.Structured.Helpers;

namespace UbiqSecurity.Tests.Internals.Structured.Helpers
{
    public class IntegerHelperTests
    {
        [Theory]
        [InlineData("70B046", 3795014)]
        [InlineData("0070B046", 3795014)]
        [InlineData("-70B046", -3795014)]
        [InlineData("-0070B046", -3795014)]
        public void Parse_Base14_ReturnsExpectedValue(string input, long expected)
        {
            var result = IntegerHelper.Parse(input, 14);

            Assert.Equal(expected, result);
        }
    }
}


using FluentAssertions;
using Xunit;

namespace AIDevs.Tests.Unit.Infrastructure
{
    public class EnvironmentVariablesTests
    {
        [Theory]
        [InlineData(EnvironmentVariableNames.AIDEVS_APIKEY_ENVIRONMENT_VARIABLE_NAME)]
        [InlineData(EnvironmentVariableNames.OPENAI_APIKEY_ENVIRONMENT_VARIABLE_NAME)]
        public void EnvironmentVariable_Should_Be_Set(string variableName)
        {
            Environment.GetEnvironmentVariable(variableName).Should().NotBeNullOrEmpty();
        }
    }
}

using NUnit.Framework;
using Utilizr.Rest.Client;

namespace Utilizr.RestClient.Tests
{
    [TestFixture(Category = "RestClient")]
    public class ApiRequestTests
    {
        [TestCase("https://token.com?token=abc123", "https://token.com?token=***")]
        [TestCase("https://token.com?foo=1&token=abc123", "https://token.com?foo=1&token=***")]
        [TestCase("https://token.com?token=abc123&foo=1", "https://token.com?token=***&foo=1")]
        [TestCase("https://token.com?foo=1&token=abc123&bar=2", "https://token.com?foo=1&token=***&bar=2")]
        [TestCase("https://token.com?foo=1&bar=2", "https://token.com?foo=1&bar=2")]
        [TestCase("https://token.com", "https://token.com")]
        [TestCase("https://token.com?TOKEN=abc123", "https://token.com?TOKEN=***")]
        public void MaskQueryParameter_ShouldMaskExpectedValue(string input, string expected)
        {
            var result = ApiRequest<object>.MaskUrlQueryParameter(input, "token");
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void MaskQueryParameter_ShouldUseCustomMask()
        {
            var input = "https://example.com?foo=1&token=abc123";
            var result = ApiRequest<object>.MaskUrlQueryParameter(input, "token", "[REDACTED]");

            Assert.That(
                result,
                Is.EqualTo("https://example.com?foo=1&token=[REDACTED]"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void MaskQueryParameter_ShouldReturnOriginal_WhenUrlIsNullOrEmpty(string? input)
        {
            var result = ApiRequest<object>.MaskUrlQueryParameter(input, "token");
            Assert.That(result, Is.EqualTo(input));
        }

        [TestCase(null)]
        [TestCase("")]
        public void MaskQueryParameter_ShouldReturnOriginal_WhenParameterNameIsNullOrEmpty(string? parameterName)
        {
            var input = "https://example.com?token=abc123";
            var result = ApiRequest<object>.MaskUrlQueryParameter(input, parameterName);
            Assert.That(result, Is.EqualTo(input));
        }


        [TestCase(@"{""Password"":""secret""}", @"{""Password"":""***""}")]
        [TestCase(@"{""Password"": ""secret""}", @"{""Password"": ""***""}")]
        [TestCase(@"{""Password"" : ""secret""}", @"{""Password"" : ""***""}")]
        [TestCase("{\r\n\"Password\"\r\n:\r\n\"secret\"\r\n}", "{\r\n\"Password\"\r\n:\r\n\"***\"\r\n}")]
        [TestCase(@"{""Username"":""bob"",""Password"":""secret""}", @"{""Username"":""bob"",""Password"":""***""}")]
        [TestCase(@"{""Password"":""secret"",""Username"":""bob""}", @"{""Password"":""***"",""Username"":""bob""}")]
        [TestCase(@"{""PASSWORD"":""secret""}", @"{""PASSWORD"":""***""}")]
        [TestCase(@"{""Username"":""bob""}", @"{""Username"":""bob""}")]
        [TestCase(@"{}", @"{}")]
        public void MaskRawJsonProperty_ShouldMaskExpectedProperty(string input, string expected)
        {
            var result = ApiRequest<object>.MaskRawJsonProperty(input, "Password");
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void MaskRawJsonProperty_ShouldUseCustomMask()
        {
            var input = @"{""Password"":""secret""}";
            var result = ApiRequest<object>.MaskRawJsonProperty(input, "Password", "[REDACTED]");
            Assert.That(
                result,
                Is.EqualTo(@"{""Password"":""[REDACTED]""}"));
        }

        [Test]
        public void MaskRawJsonProperty_ShouldMaskAllMatchingProperties()
        {
            var input = @"{""Password"":""first"", ""Nested"":{""Password"":""second""}}";

            var result = ApiRequest<object>.MaskRawJsonProperty(input, "Password");

            Assert.That(result, Does.Not.Contain("first"));
            Assert.That(result, Does.Not.Contain("second"));
            Assert.That(result, Is.EqualTo(@"{""Password"":""***"", ""Nested"":{""Password"":""***""}}"));
        }

        [Test]
        public void MaskRawJsonProperty_ShouldNotMaskPartialPropertyNames()
        {
            var input = @"{""Password"":""secret"", ""PasswordHash"":""abc123""}";
            var result = ApiRequest<object>.MaskRawJsonProperty(input, "Password");

            Assert.That(result, Does.Contain(@"""PasswordHash"":""abc123"""));
            Assert.That(result, Does.Contain(@"""Password"":""***"""));
        }
    }
}
using WakeOnLanLibrary.Core.Validation;

namespace WakeOnLanLibrary.Tests.Validators.Helpers
{
    public class NameIpValidatorTests
    {
        private readonly NameIpValidator _validator;

        public NameIpValidatorTests()
        {
            _validator = new NameIpValidator();
        }

        [Theory]
        [InlineData("ValidComputerName", true)]
        [InlineData("123-Valid-Name", true)]
        [InlineData("Name123", true)]
        [InlineData("Invalid_Name!", false)]
        [InlineData("ThisNameIsWayTooLongToBeAValidComputerNameBecauseItExceedsTheSixtyThreeCharacterLimit", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsValidComputerName_ValidatesCorrectly(string name, bool expected)
        {
            // Act
            var result = _validator.IsValidComputerName(name);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("192.168.1.1", true)] // Valid IPv4
        [InlineData("::1", true)] // Valid IPv6
        [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334", true)] // Valid IPv6
        [InlineData("999.999.999.999", false)] // Invalid IPv4
        [InlineData("NotAnIpAddress", false)] // Invalid string
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsValidIpAddress_ValidatesCorrectly(string ipAddress, bool expected)
        {
            // Act
            var result = _validator.IsValidIpAddress(ipAddress);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}

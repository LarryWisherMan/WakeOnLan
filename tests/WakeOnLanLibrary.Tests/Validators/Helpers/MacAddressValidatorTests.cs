namespace WakeOnLanLibrary.Tests.Validators.Helpers
{
    public class MacAddressValidatorTests
    {
        private readonly MacAddressValidator _validator;

        public MacAddressValidatorTests()
        {
            _validator = new MacAddressValidator();
        }

        [Theory]
        [InlineData("00:1A:2B:3C:4D:5E", true)]
        [InlineData("00-1A-2B-3C-4D-5E", true)]
        [InlineData("001A2B3C4D5E", false)]
        [InlineData("00:1G:2B:3C:4D:5E", false)] // Invalid hex character
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsValidMacAddress_ValidatesCorrectly(string macAddress, bool expected)
        {
            // Act
            var result = _validator.IsValidMacAddress(macAddress);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("00:1A:2B:3C:4D:5E", new byte[] { 0x00, 0x1A, 0x2B, 0x3C, 0x4D, 0x5E })]
        [InlineData("00-1A-2B-3C-4D-5E", new byte[] { 0x00, 0x1A, 0x2B, 0x3C, 0x4D, 0x5E })]
        public void ParseMacAddress_WhenValid_ReturnsCorrectByteArray(string macAddress, byte[] expected)
        {
            // Act
            var result = _validator.ParseMacAddress(macAddress);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("001A2B3C4D5E")] // Invalid format (no separators)
        [InlineData("00:1G:2B:3C:4D:5E")] // Invalid hex character
        [InlineData("")] // Empty string
        [InlineData(null)] // Null string
        public void ParseMacAddress_WhenInvalid_ThrowsArgumentException(string macAddress)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _validator.ParseMacAddress(macAddress));

            // Assert: Validate exception message
            Assert.StartsWith("Invalid MAC address format.", exception.Message);
        }
    }
}

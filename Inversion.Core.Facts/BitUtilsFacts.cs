using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Inversion.Utils;

namespace Inversion.Core.Facts
{
    public class BitUtilsFacts
    {
        public class FromHexString
        {
            [Fact]
            public void ThrowsOnNullOrEmptyInput()
            {
                Assert.Throws<ArgumentException>(() => BitUtils.FromHexString(null))
                      .WithParamName("value")
                      .WithMessage(String.Format(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "value"));
                Assert.Throws<ArgumentException>(() => BitUtils.FromHexString(String.Empty))
                      .WithParamName("value")
                      .WithMessage(String.Format(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "value"));
            }

            [Fact]
            public void ThrowsOnInvalidCharacter()
            {
                Assert.Throws<FormatException>(() => BitUtils.FromHexString("zzzz"))
                      .WithMessage(String.Format(CoreResources.Input_Not_Valid_Hex_String, "zzzz"));
            }

            [Fact]
            public void CorrectlyConvertsHexString()
            {
                // Arrange
                byte[] expected = new byte[] {
                    0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0xAB, 0xCD, 0xEF
                };

                // Act
                byte[] actual = BitUtils.FromHexString("0123456789abcdefABCDEF");

                // Assert
                Assert.Equal(expected, actual);
            }
        }
    }
}

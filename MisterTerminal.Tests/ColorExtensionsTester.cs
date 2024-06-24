namespace MisterTerminal.Tests;

[TestClass]
public class ColorExtensionsTester
{
    [TestClass]
    public class ToAnsiTrueColor : Tester
    {
        [TestMethod]
        public void Always_ReturnAnsiTrueColor()
        {
            //Arrange
            var color = Dummy.Create<Color>();

            //Act
            var result = color.ToAnsiTrueColor();

            //Assert
            result.Should().Be($"\x1b[38;2;{color.Red};{color.Green};{color.Blue}m");
        }
    }

    [TestClass]
    public class ToAnsiTrueColorHighlight : Tester
    {
        [TestMethod]
        public void Always_ReturnAnsiTrueColor()
        {
            //Arrange
            var color = Dummy.Create<Color>();

            //Act
            var result = color.ToAnsiTrueColorHighlight();

            //Assert
            result.Should().Be($"\x1b[48;2;{color.Red};{color.Green};{color.Blue}m");
        }
    }
}
namespace MisterTerminal.Tests;

[TestClass]
public class DmlAnsiConverterTester
{
    [TestClass]
    public class Convert : Tester<DmlAnsiConverter>
    {
        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow(null)]
        public void WhenValueIsNullOrEmpty_Throw(string value)
        {
            //Arrange

            //Act
            var action = () => Instance.Convert(value);

            //Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenContainsColorTags_ConvertToProperAnsiColorCode()
        {
            //Arrange
            var color = Fixture.Create<Color>();
            var value = Fixture.Create<string>();

            var dmlString = new DmlString(new DmlSubstringEntry[]
            {
                new(new DmlSubstring { Text = "Some " }),
                new(new DmlSubstring { Text = "texty", Color = color}),
                new(new DmlSubstring { Text = " text" }),
            });
            GetMock<IDmlSerializer>().Setup(x => x.Deserialize(value)).Returns(dmlString);

            //Act
            var result = Instance.Convert(value);

            //Assert
            result.Should().BeEquivalentTo($"Some \x1b[38;2;{color.Red};{color.Green};{color.Blue}mtexty\u001b[0m text");
        }

        [TestMethod]
        public void WhenContainsColorTagsButEnvironmentVariableNoColorIsSet_DoNotAddAnyColorCodes()
        {
            //Arrange
            GetMock<IEnvironmentVariables>().Setup(x => x.Contains(EnvironmentVariableNames.NoColor)).Returns(true);
            var color = Fixture.Create<Color>();
            var value = Fixture.Create<string>();

            var dmlString = new DmlString(new DmlSubstringEntry[]
            {
                new(new DmlSubstring { Text = "Some " }),
                new(new DmlSubstring { Text = "texty", Color = color}),
                new(new DmlSubstring { Text = " text" }),
            });
            GetMock<IDmlSerializer>().Setup(x => x.Deserialize(value)).Returns(dmlString);

            //Act
            var result = Instance.Convert(value);

            //Assert
            result.Should().BeEquivalentTo("Some texty text");
        }

        [TestMethod]
        public void WhenContainsBoldTags_ConvertToProperAnsiCode()
        {
            //Arrange
            var value = Fixture.Create<string>();

            var dmlString = new DmlString(new DmlSubstringEntry[]
            {
                new(new DmlSubstring { Text = "Some " }),
                new(new DmlSubstring { Text = "very", Styles = new List<TextStyle> { TextStyle.Bold }}),
                new(new DmlSubstring { Text = " texty text" }),
            });
            GetMock<IDmlSerializer>().Setup(x => x.Deserialize(value)).Returns(dmlString);

            //Act
            var result = Instance.Convert(value);

            //Assert
            result.Should().Be("Some \u001b[1mvery\u001b[0m texty text");
        }

        [TestMethod]
        public void WhenContainsUnderlineTags_ConvertToProperAnsiCode()
        {
            //Arrange
            var value = Fixture.Create<string>();

            var dmlString = new DmlString(new DmlSubstringEntry[]
            {
                new(new DmlSubstring { Text = "Some " }),
                new(new DmlSubstring { Text = "very", Styles = new List<TextStyle> { TextStyle.Underline }}),
                new(new DmlSubstring { Text = " texty text" }),
            });
            GetMock<IDmlSerializer>().Setup(x => x.Deserialize(value)).Returns(dmlString);

            //Act
            var result = Instance.Convert(value);

            //Assert
            result.Should().Be("Some \u001b[4mvery\u001b[0m texty text");
        }

        [TestMethod]
        public void WhenContainsItalicTags_ConvertToProperAnsiCode()
        {
            //Arrange
            var value = Fixture.Create<string>();

            var dmlString = new DmlString(new DmlSubstringEntry[]
            {
                new(new DmlSubstring { Text = "Some " }),
                new(new DmlSubstring { Text = "very", Styles = new List<TextStyle> { TextStyle.Italic }}),
                new(new DmlSubstring { Text = " texty text" }),
            });
            GetMock<IDmlSerializer>().Setup(x => x.Deserialize(value)).Returns(dmlString);

            //Act
            var result = Instance.Convert(value);

            //Assert
            result.Should().Be("Some \u001b[3mvery\u001b[0m texty text");
        }

        [TestMethod]
        public void WhenContainsStrikeoutTags_ConvertToProperAnsiCode()
        {
            //Arrange
            var value = Fixture.Create<string>();

            var dmlString = new DmlString(new DmlSubstringEntry[]
            {
                new(new DmlSubstring { Text = "Some " }),
                new(new DmlSubstring { Text = "very", Styles = new List<TextStyle> { TextStyle.Strikeout }}),
                new(new DmlSubstring { Text = " texty text" }),
            });
            GetMock<IDmlSerializer>().Setup(x => x.Deserialize(value)).Returns(dmlString);

            //Act
            var result = Instance.Convert(value);

            //Assert
            result.Should().Be("Some \u001b[9mvery\u001b[0m texty text");
        }
    }
}
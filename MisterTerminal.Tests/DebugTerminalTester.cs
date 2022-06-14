using ConsoleColor = System.ConsoleColor;

namespace MisterTerminal.Tests;
#if DEBUG
[TestClass]
public class DebugTerminalTester
{
    [TestClass]
    public class Constructor : Tester<DebugTerminal>
    {
        [TestMethod]
        public void WhenBackgroundColorIsNullInSettings_UseGlobalDefaultColor()
        {
            //Arrange
            SetupOptions(Fixture.Create<TerminalSettings>() with { Debug = null });

            //Act
            //Constructor

            //Assert
            Instance.BackgroundColor.Should().Be(DefaultColors.Debug.Background!.Value);
        }

        [TestMethod]
        public void WhenForeroundColorIsNullInSettings_UseGlobalDefaultColor()
        {
            //Arrange
            SetupOptions(Fixture.Create<TerminalSettings>() with { Debug = null });

            //Act
            //Constructor

            //Assert
            Instance.ForegroundColor.Should().Be(DefaultColors.Debug.Foreground!.Value);
        }

        [TestMethod]
        public void WhenBackgroundColorInSettingsIsNotNull_UseThat()
        {
            //Arrange
            var options = SetupOptions<TerminalSettings>();

            //Act
            //Constructor

            //Assert
            Instance.BackgroundColor.Should().Be(options.Debug!.Color!.Background!.Value);
        }

        [TestMethod]
        public void WhenForegroundColorInSettingsIsNotNull_UseThat()
        {
            //Arrange
            var options = SetupOptions<TerminalSettings>();

            //Act
            //Constructor

            //Assert
            Instance.ForegroundColor.Should().Be(options.Debug!.Color!.Foreground!.Value);
        }
    }

    [TestClass]
    public class BackgroundColor : Tester<DebugTerminal>
    {
        protected override void InitializeTest()
        {
            base.InitializeTest();
            var options = Fixture.Create<TerminalSettings>();
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
        }

        [TestMethod]
        public void WhenSettingColor_ReturnNewColor()
        {
            //Arrange
            var value = Fixture.Create<Color>();

            //Act
            Instance.BackgroundColor = value;

            //Assert
            Instance.BackgroundColor.Should().Be(value);
        }

        //TODO Test
        [TestMethod]
        [Ignore("Background colors are not yet supported in DML")]
        public void WhenWriting_ApplyColorToText()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);

            var value = Fixture.Create<Color>();
            Instance.BackgroundColor = value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Highlight(value);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.Write(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }
    }

    [TestClass]
    public class ForegroundColor : Tester<DebugTerminal>
    {
        protected override void InitializeTest()
        {
            base.InitializeTest();
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
        }
        
        [TestMethod]
        public void WhenSettingColor_ReturnNewColor()
        {
            //Arrange
            var value = Fixture.Create<Color>();

            //Act
            Instance.ForegroundColor = value;

            //Assert
            Instance.ForegroundColor.Should().Be(value);
        }

        [TestMethod]
        public void WhenWriting_ApplyColorToText()
        {
            //Arrange
            var value = Fixture.Create<Color>();
            Instance.ForegroundColor = value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(value);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.Write(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }
    }

    [TestClass]
    public class Write : Tester<DebugTerminal>
    {
        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow(null)]
        public void WhenTextIsNullOrWhiteSpace_Throw(string text)
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>();
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);

            var args = Fixture.CreateMany<object>().ToArray();

            //Act
            var action = () => Instance.Write(text, args);

            //Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenUsingTimestamps_IncludeTimestamps()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = true } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Debug.Color!.Foreground!.Value;

            var now = Fixture.Create<DateTime>();
            TimeProvider.Freeze(now);

            var timestamp = string.Format($"[{TimeProvider.Now.ToString(options.TimeStamps.Format)}]");

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = $"{timestamp} {text}".Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.Write(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void WhenNotUsingTimestamps_DoNotIncludeTimestamps()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Debug.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.Write(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void WhenIsFormattableString_FormatString()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Debug.Color!.Foreground!.Value;

            var text = "This {0} is {1} formattable";

            var textWithColor = "This string is very formattable".Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.Write(text, "string", "very");

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void Always_TriggerWroteEvent()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Debug.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var triggers = new List<WriteEventArgs>();
            Instance.Wrote += (sender, args) => triggers.Add(args);

            //Act
            Instance.Write(text, args);

            //Assert
            triggers.Should().BeEquivalentTo(new List<WriteEventArgs> { new() { Text = formattedText } });
        }
    }

    [TestClass]
    public class TryWrite : Tester<DebugTerminal>
    {
        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow(null)]
        public void WhenTextIsNullOrWhiteSpace_DoNotThrow(string text)
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>();
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);

            var args = Fixture.CreateMany<object>().ToArray();

            //Act
            var action = () => Instance.TryWrite(text, args);

            //Assert
            action.Should().NotThrow();
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow(null)]
        public void WhenTextIsNullOrWhiteSpace_DoNotWriteToConsole(string text)
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>();
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);

            var args = Fixture.CreateMany<object>().ToArray();

            //Act
            Instance.TryWrite(text, args);

            //Assert
            GetMock<IConsole>().VerifySet(x => x.ForegroundColor = ConsoleColor.Gray);
            GetMock<IConsole>().VerifyNoOtherCalls();
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow(null)]
        public void WhenTextIsNullOrWhiteSpace_DoNotTriggerEvent(string text)
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>();
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);

            var args = Fixture.CreateMany<object>().ToArray();

            var triggers = new List<WriteEventArgs>();
            Instance.Wrote += (sender, eventArgs) => triggers.Add(eventArgs);

            //Act
            Instance.TryWrite(text, args);

            //Assert
            triggers.Should().BeEmpty();
        }

        [TestMethod]
        public void WhenUsingTimestamps_IncludeTimestamps()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = true } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Debug.Color!.Foreground!.Value;

            var now = Fixture.Create<DateTime>();
            TimeProvider.Freeze(now);

            var timestamp = string.Format($"[{TimeProvider.Now.ToString(options.TimeStamps.Format)}]");

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = $"{timestamp} {text}".Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.TryWrite(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void WhenNotUsingTimestamps_DoNotIncludeTimestamps()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Debug.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.TryWrite(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void WhenIsFormattableString_FormatString()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Debug.Color!.Foreground!.Value;

            var text = "This {0} is {1} formattable";

            var textWithColor = "This string is very formattable".Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.TryWrite(text, "string", "very");

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void Always_TriggerWroteEvent()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Debug.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var triggers = new List<WriteEventArgs>();
            Instance.Wrote += (sender, args) => triggers.Add(args);

            //Act
            Instance.TryWrite(text, args);

            //Assert
            triggers.Should().BeEquivalentTo(new List<WriteEventArgs> { new() { Text = formattedText } });
        }
    }

    [TestClass]
    public class ResetColor : Tester<DebugTerminal>
    {
        [TestMethod]
        public void Always_ResetBackgroundColorToDefault()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>();
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);

            Instance.BackgroundColor = Fixture.Create<Color>();

            //Act
            Instance.ResetColor();

            //Assert
            Instance.BackgroundColor.Should().Be(options!.Debug.Color!.Background);

        }

        [TestMethod]
        public void Always_ResetForegroundColorToDefault()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>();
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);

            Instance.ForegroundColor = Fixture.Create<Color>();

            //Act
            Instance.ResetColor();

            //Assert
            Instance.ForegroundColor.Should().Be(options!.Debug.Color!.Foreground);
        }
    }
}
#endif
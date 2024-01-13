namespace MisterTerminal.Tests;

public abstract class TerminalTesterBase : Tester<Terminal>
{
    protected ConsoleKey YesKey;
    protected ConsoleKey NoKey;
    protected Color ForegroundColor;

    protected override void InitializeTest()
    {
        base.InitializeTest();

        var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
        SetupOptions(options);
        ForegroundColor = options.Main!.Color!.Foreground!.Value;
        GetMock<IConsole>().Setup(x => x.ReadLine()).Returns(Fixture.Create<string>());
        GetMock<IConsole>().Setup(x => x.ReadAndInterceptKey()).Returns(Fixture.Create<ConsoleKeyInfo>());
        GetMock<IConsole>().Setup(x => x.ReadKey()).Returns(Fixture.Create<ConsoleKeyInfo>());

        YesKey = (ConsoleKey)Text.Yes.ToUpperInvariant()[0];
        NoKey = (ConsoleKey)Text.No.ToUpperInvariant()[0];
    }
}

[TestClass]
public class TerminalTester
{
    [TestClass]
    public class Constructor : TerminalTesterBase
    {
        [TestMethod]
        public void WhenBackgroundColorIsNullInSettings_UseGlobalDefaultColor()
        {
            //Arrange
            SetupOptions(Fixture.Create<TerminalSettings>() with { Main = null });

            //Act
            //Constructor

            //Assert
            Instance.BackgroundColor.Should().Be(DefaultColors.Main.Background!.Value);
        }

        [TestMethod]
        public void WhenForeroundColorIsNullInSettings_UseGlobalDefaultColor()
        {
            //Arrange
            SetupOptions(Fixture.Create<TerminalSettings>() with { Main = null });

            //Act
            //Constructor

            //Assert
            Instance.ForegroundColor.Should().Be(DefaultColors.Main.Foreground!.Value);
        }

        [TestMethod]
        public void WhenBackgroundColorInSettingsIsNotNull_UseThat()
        {
            //Arrange
            var options = SetupOptions<TerminalSettings>();

            //Act
            //Constructor

            //Assert
            Instance.BackgroundColor.Should().Be(options.Main!.Color!.Background!.Value);
        }

        [TestMethod]
        public void WhenForegroundColorInSettingsIsNotNull_UseThat()
        {
            //Arrange
            var options = SetupOptions<TerminalSettings>();

            //Act
            //Constructor

            //Assert
            Instance.ForegroundColor.Should().Be(options.Main!.Color!.Foreground!.Value);
        }
    }

    [TestClass]
    public class BackgroundColor : TerminalTesterBase
    {
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

        [TestMethod]
        [Ignore("Background colors are not yet supported in DML")]
        public void WhenWriting_ApplyColorToText()
        {
            //Arrange

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
    public class ForegroundColor : TerminalTesterBase
    {
        protected override void InitializeTest()
        {
            base.InitializeTest();
            SetupOptions(Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } });
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
    public class Write : TerminalTesterBase
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
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var now = Fixture.Create<DateTime>();
            GlobalTimeProvider.Freeze(now);

            var timestamp = string.Format($"[{GlobalTimeProvider.Now.ToString(options.TimeStamps.Format)}]");

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
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

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
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

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
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var triggers = new List<WriteEventArgs>();
            Instance.Wrote += (_, eventArgs) => triggers.Add(eventArgs);

            //Act
            Instance.Write(text, args);

            //Assert
            triggers.Should().BeEquivalentTo(new List<WriteEventArgs> { new() { Text = formattedText } });
        }
    }

    [TestClass]
    public class TryWrite : TerminalTesterBase
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
            Instance.Wrote += (_, eventArgs) => triggers.Add(eventArgs);

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
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var now = Fixture.Create<DateTime>();
            GlobalTimeProvider.Freeze(now);

            var timestamp = string.Format($"[{GlobalTimeProvider.Now.ToString(options.TimeStamps.Format)}]");

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
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

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
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

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
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var triggers = new List<WriteEventArgs>();
            Instance.Wrote += (_, eventArgs) => triggers.Add(eventArgs);

            //Act
            Instance.TryWrite(text, args);

            //Assert
            triggers.Should().BeEquivalentTo(new List<WriteEventArgs> { new() { Text = formattedText } });
        }
    }

    [TestClass]
    public class ResetColor : TerminalTesterBase
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
            Instance.BackgroundColor.Should().Be(options!.Main!.Color!.Background);

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
            Instance.ForegroundColor.Should().Be(options!.Main!.Color!.Foreground);
        }
    }

    [TestClass]
    public class Debug : TerminalTesterBase
    {
        [TestMethod]
        public void Always_ReturnInstanceOfDebugTerminal()
        {
            //Arrange

            //Act
            var result = Instance.Debug;

            //Assert
            result.Should().Be(GetMock<IDebugTerminal>().Object);
        }
    }

    [TestClass]
    public class Notification : TerminalTesterBase
    {
        [TestMethod]
        public void Always_ReturnInstanceOfNotificationTerminal()
        {
            //Arrange

            //Act
            var result = Instance.Notification;

            //Assert
            result.Should().Be(GetMock<INotificationTerminal>().Object);
        }
    }

    [TestClass]
    public class Error : TerminalTesterBase
    {
        [TestMethod]
        public void Always_ReturnInstanceOfErrorTerminal()
        {
            //Arrange

            //Act
            var result = Instance.Error;

            //Assert
            result.Should().Be(GetMock<IErrorTerminal>().Object);
        }
    }

    [TestClass]
    public class Warning : TerminalTesterBase
    {
        [TestMethod]
        public void Always_ReturnInstanceOfWarningTerminal()
        {
            //Arrange

            //Act
            var result = Instance.Warning;

            //Assert
            result.Should().Be(GetMock<IWarningTerminal>().Object);
        }
    }

    [TestClass]
    public class Ask : TerminalTesterBase
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
            var action = () => Instance.Ask(text, args);

            //Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenUsingTimestamps_IncludeTimestamps()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = true } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var now = Fixture.Create<DateTime>();
            GlobalTimeProvider.Freeze(now);

            var timestamp = string.Format($"[{GlobalTimeProvider.Now.ToString(options.TimeStamps.Format)}]");

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = $"{timestamp} {text}".Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.Ask(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void WhenNotUsingTimestamps_DoNotIncludeTimestamps()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.Ask(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void WhenIsFormattableString_FormatString()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = "This {0} is {1} formattable";

            var textWithColor = "This string is very formattable".Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.Ask(text, "string", "very");

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void Always_TriggerWroteEvent()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var triggers = new List<WriteEventArgs>();
            Instance.Wrote += (_, eventArgs) => triggers.Add(eventArgs);

            //Act
            Instance.Ask(text, args);

            //Assert
            triggers.Should().BeEquivalentTo(new List<WriteEventArgs> { new() { Text = formattedText } });
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow(null)]
        public void WhenResponseIsNullOrEmpty_KeepAskingUntilItsNot(string response)
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var validResponse = Fixture.Create<string>();
            GetMock<IConsole>().SetupSequence(x => x.ReadLine()).Returns(response).Returns(response).Returns(response).Returns(validResponse);

            //Act
            var result = Instance.Ask(text, args);

            //Assert
            result.Should().Be(validResponse);
        }

        [TestMethod]
        public void WhenResponseIsValidRightAway_ReturnThat()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var response = Fixture.Create<string>();
            GetMock<IConsole>().SetupSequence(x => x.ReadLine()).Returns(response);

            //Act
            var result = Instance.Ask(text, args);

            //Assert
            result.Should().Be(response);
        }
    }

    [TestClass]
    public class AskOnSameLine : TerminalTesterBase
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
            var action = () => Instance.AskOnSameLine(text, args);

            //Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenUsingTimestamps_IncludeTimestamps()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = true } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var now = Fixture.Create<DateTime>();
            GlobalTimeProvider.Freeze(now);

            var timestamp = string.Format($"[{GlobalTimeProvider.Now.ToString(options.TimeStamps.Format)}]");

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = $"{timestamp} {text}".Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.AskOnSameLine(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void WhenNotUsingTimestamps_DoNotIncludeTimestamps()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.AskOnSameLine(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void WhenIsFormattableString_FormatString()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = "This {0} is {1} formattable";

            var textWithColor = "This string is very formattable".Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.AskOnSameLine(text, "string", "very");

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void Always_TriggerWroteEvent()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var triggers = new List<WriteEventArgs>();
            Instance.Wrote += (_, eventArgs) => triggers.Add(eventArgs);

            //Act
            Instance.AskOnSameLine(text, args);

            //Assert
            triggers.Should().BeEquivalentTo(new List<WriteEventArgs> { new() { Text = formattedText } });
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow(null)]
        public void WhenResponseIsNullOrEmpty_KeepAskingUntilItsNot(string response)
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var validResponse = Fixture.Create<string>();
            GetMock<IConsole>().SetupSequence(x => x.ReadLine()).Returns(response).Returns(response).Returns(response).Returns(validResponse);

            //Act
            var result = Instance.AskOnSameLine(text, args);

            //Assert
            result.Should().Be(validResponse);
        }

        [TestMethod]
        public void WhenResponseIsValidRightAway_ReturnThat()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var response = Fixture.Create<string>();
            GetMock<IConsole>().SetupSequence(x => x.ReadLine()).Returns(response);

            //Act
            var result = Instance.AskOnSameLine(text, args);

            //Assert
            result.Should().Be(response);
        }

        [TestMethod]
        public void Always_DoNotBreakLineUntilAnswer()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var response = Fixture.Create<string>();
            GetMock<IConsole>().SetupSequence(x => x.ReadLine()).Returns(response);

            //Act
            Instance.AskOnSameLine(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.WriteLine(), Times.Never);
        }
    }

    [TestClass]
    public class TryAsk : TerminalTesterBase
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
            var action = () => Instance.TryAsk(text, args);

            //Assert
            action.Should().NotThrow();
        }

        [TestMethod]
        public void WhenUsingTimestamps_IncludeTimestamps()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = true } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var now = Fixture.Create<DateTime>();
            GlobalTimeProvider.Freeze(now);

            var timestamp = string.Format($"[{GlobalTimeProvider.Now.ToString(options.TimeStamps.Format)}]");

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = $"{timestamp} {text}".Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.TryAsk(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void WhenNotUsingTimestamps_DoNotIncludeTimestamps()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.TryAsk(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void WhenIsFormattableString_FormatString()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = "This {0} is {1} formattable";

            var textWithColor = "This string is very formattable".Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.TryAsk(text, "string", "very");

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void Always_TriggerWroteEvent()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var triggers = new List<WriteEventArgs>();
            Instance.Wrote += (_, eventArgs) => triggers.Add(eventArgs);

            //Act
            Instance.TryAsk(text, args);

            //Assert
            triggers.Should().BeEquivalentTo(new List<WriteEventArgs> { new() { Text = formattedText } });
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(null)]
        public void WhenResponseIsNullOrEmpty_ReturnEmptyString(string response)
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var validResponse = Fixture.Create<string>();
            GetMock<IConsole>().SetupSequence(x => x.ReadLine()).Returns(response).Returns(response).Returns(response).Returns(validResponse);

            //Act
            var result = Instance.TryAsk(text, args);

            //Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void WhenResponseIsWhiteSpace_ReturnWhiteSpace()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            GetMock<IConsole>().SetupSequence(x => x.ReadLine()).Returns("  ");

            //Act
            var result = Instance.TryAsk(text, args);

            //Assert
            result.Should().Be("  ");
        }

        [TestMethod]
        public void WhenResponseIsValidRightAway_ReturnThat()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var response = Fixture.Create<string>();
            GetMock<IConsole>().SetupSequence(x => x.ReadLine()).Returns(response);

            //Act
            var result = Instance.TryAsk(text, args);

            //Assert
            result.Should().Be(response);
        }
    }

    [TestClass]
    public class TryAskOnSameLine : TerminalTesterBase
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
            var action = () => Instance.TryAskOnSameLine(text, args);

            //Assert
            action.Should().NotThrow();
        }

        [TestMethod]
        public void WhenUsingTimestamps_IncludeTimestamps()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = true } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var now = Fixture.Create<DateTime>();
            GlobalTimeProvider.Freeze(now);

            var timestamp = string.Format($"[{GlobalTimeProvider.Now.ToString(options.TimeStamps.Format)}]");

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = $"{timestamp} {text}".Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.TryAskOnSameLine(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void WhenNotUsingTimestamps_DoNotIncludeTimestamps()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.TryAskOnSameLine(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void WhenIsFormattableString_FormatString()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = "This {0} is {1} formattable";

            var textWithColor = "This string is very formattable".Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.TryAskOnSameLine(text, "string", "very");

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void Always_TriggerWroteEvent()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var triggers = new List<WriteEventArgs>();
            Instance.Wrote += (_, eventArgs) => triggers.Add(eventArgs);

            //Act
            Instance.TryAskOnSameLine(text, args);

            //Assert
            triggers.Should().BeEquivalentTo(new List<WriteEventArgs> { new() { Text = formattedText } });
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(null)]
        public void WhenResponseIsNullOrEmpty_ReturnEmpty(string response)
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var validResponse = Fixture.Create<string>();
            GetMock<IConsole>().SetupSequence(x => x.ReadLine()).Returns(response).Returns(response).Returns(response).Returns(validResponse);

            //Act
            var result = Instance.TryAskOnSameLine(text, args);

            //Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void WhenResponseIsWhiteSpace_ReturnThat()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var response = "  ";
            GetMock<IConsole>().SetupSequence(x => x.ReadLine()).Returns(response);

            //Act
            var result = Instance.TryAskOnSameLine(text, args);

            //Assert
            result.Should().Be(response);
        }

        [TestMethod]
        public void WhenResponseIsValidRightAway_ReturnThat()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var response = Fixture.Create<string>();
            GetMock<IConsole>().SetupSequence(x => x.ReadLine()).Returns(response);

            //Act
            var result = Instance.TryAskOnSameLine(text, args);

            //Assert
            result.Should().Be(response);
        }

        [TestMethod]
        public void Always_DoNotBreakLineUntilAnswer()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var response = Fixture.Create<string>();
            GetMock<IConsole>().SetupSequence(x => x.ReadLine()).Returns(response);

            //Act
            Instance.TryAskOnSameLine(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.WriteLine(), Times.Never);
        }
    }

    [TestClass]
    public class AskSecret : TerminalTesterBase
    {
        protected override void InitializeTest()
        {
            base.InitializeTest();
            GetMock<IConsole>().Setup(x => x.ReadAndInterceptKey()).Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Enter, false, false, false));
        }

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
            var action = () => Instance.AskSecret(text, args);

            //Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenUsingTimestamps_IncludeTimestamps()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = true } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var now = Fixture.Create<DateTime>();
            GlobalTimeProvider.Freeze(now);

            var timestamp = string.Format($"[{GlobalTimeProvider.Now.ToString(options.TimeStamps.Format)}]");

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = $"{timestamp} {text}".Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.AskSecret(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void WhenNotUsingTimestamps_DoNotIncludeTimestamps()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.AskSecret(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void WhenIsFormattableString_FormatString()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = "This {0} is {1} formattable";

            var textWithColor = "This string is very formattable".Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.AskSecret(text, "string", "very");

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void Always_TriggerWroteEvent()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var triggers = new List<WriteEventArgs>();
            Instance.Wrote += (_, eventArgs) => triggers.Add(eventArgs);

            //Act
            Instance.AskSecret(text, args);

            //Assert
            triggers.Should().BeEquivalentTo(new List<WriteEventArgs> { new() { Text = formattedText } });
        }

        [TestMethod]
        public void WhenPressingEnterRightAway_ReturnEmptySecret()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            GetMock<IConsole>().Setup(x => x.ReadAndInterceptKey()).Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Enter, false, false, false));

            //Act
            var result = Instance.AskSecret(text, args);

            //Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void WhenEnteringTextBeforePressingEnter_ReturnAllText()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            GetMock<IConsole>().SetupSequence(x => x.ReadAndInterceptKey())
                .Returns(new ConsoleKeyInfo('w', ConsoleKey.W, false, false, false))
                .Returns(new ConsoleKeyInfo('t', ConsoleKey.T, false, false, false))
                .Returns(new ConsoleKeyInfo('f', ConsoleKey.F, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Enter, false, false, false));

            //Act
            var result = Instance.AskSecret(text, args);

            //Assert
            result.Should().Be("wtf");
        }

        [TestMethod]
        public void WhenUsingBackspaceAsFirstCharacter_DoNothing()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            GetMock<IConsole>().SetupSequence(x => x.ReadAndInterceptKey())
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Backspace, false, false, false))
                .Returns(new ConsoleKeyInfo('w', ConsoleKey.W, false, false, false))
                .Returns(new ConsoleKeyInfo('t', ConsoleKey.T, false, false, false))
                .Returns(new ConsoleKeyInfo('f', ConsoleKey.F, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Enter, false, false, false));

            //Act
            var result = Instance.AskSecret(text, args);

            //Assert
            result.Should().Be("wtf");
        }

        [TestMethod]
        public void WhenUsingBackspace_DeleteLastCharacter()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            GetMock<IConsole>().SetupSequence(x => x.ReadAndInterceptKey())
                .Returns(new ConsoleKeyInfo('w', ConsoleKey.W, false, false, false))
                .Returns(new ConsoleKeyInfo('t', ConsoleKey.T, false, false, false))
                .Returns(new ConsoleKeyInfo('f', ConsoleKey.F, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Backspace, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Enter, false, false, false));

            //Act
            var result = Instance.AskSecret(text, args);

            //Assert
            result.Should().Be("wt");
        }

        [TestMethod]
        public void Always_IgnoreControlKeys()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            GetMock<IConsole>().SetupSequence(x => x.ReadAndInterceptKey())
                .Returns(new ConsoleKeyInfo('w', ConsoleKey.W, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.LeftArrow, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Escape, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.UpArrow, false, false, false))
                .Returns(new ConsoleKeyInfo('t', ConsoleKey.T, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.DownArrow, false, false, false))
                .Returns(new ConsoleKeyInfo('f', ConsoleKey.F, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.RightArrow, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.PageUp, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.PageDown, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Enter, false, false, false));

            //Act
            var result = Instance.AskSecret(text, args);

            //Assert
            result.Should().Be("wtf");
        }
    }

    [TestClass]
    public class TryAskSecret : TerminalTesterBase
    {
        protected override void InitializeTest()
        {
            base.InitializeTest();
            GetMock<IConsole>().Setup(x => x.ReadAndInterceptKey()).Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Enter, false, false, false));
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow(null)]
        public void WhenTextIsNullOrWhiteSpace_DoNotThrow(string text)
        {
            //Arrange

            var args = Fixture.CreateMany<object>().ToArray();

            //Act
            var action = () => Instance.TryAskSecret(text, args);

            //Assert
            action.Should().NotThrow();
        }

        [TestMethod]
        public void WhenUsingTimestamps_IncludeTimestamps()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = true } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var now = Fixture.Create<DateTime>();
            GlobalTimeProvider.Freeze(now);

            var timestamp = string.Format($"[{GlobalTimeProvider.Now.ToString(options.TimeStamps.Format)}]");

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = $"{timestamp} {text}".Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.TryAskSecret(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void WhenNotUsingTimestamps_DoNotIncludeTimestamps()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.TryAskSecret(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void WhenIsFormattableString_FormatString()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = "This {0} is {1} formattable";

            var textWithColor = "This string is very formattable".Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.TryAskSecret(text, "string", "very");

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void Always_TriggerWroteEvent()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var triggers = new List<WriteEventArgs>();
            Instance.Wrote += (_, eventArgs) => triggers.Add(eventArgs);

            //Act
            Instance.TryAskSecret(text, args);

            //Assert
            triggers.Should().BeEquivalentTo(new List<WriteEventArgs> { new() { Text = formattedText } });
        }

        [TestMethod]
        public void WhenPressingEnterRightAway_ReturnEmptySecret()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            GetMock<IConsole>().Setup(x => x.ReadAndInterceptKey()).Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Enter, false, false, false));

            //Act
            var result = Instance.TryAskSecret(text, args);

            //Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void WhenEnteringTextBeforePressingEnter_ReturnAllText()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            GetMock<IConsole>().SetupSequence(x => x.ReadAndInterceptKey())
                .Returns(new ConsoleKeyInfo('w', ConsoleKey.W, false, false, false))
                .Returns(new ConsoleKeyInfo('t', ConsoleKey.T, false, false, false))
                .Returns(new ConsoleKeyInfo('f', ConsoleKey.F, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Enter, false, false, false));

            //Act
            var result = Instance.TryAskSecret(text, args);

            //Assert
            result.Should().Be("wtf");
        }

        [TestMethod]
        public void WhenUsingBackspaceAsFirstCharacter_DoNothing()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            GetMock<IConsole>().SetupSequence(x => x.ReadAndInterceptKey())
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Backspace, false, false, false))
                .Returns(new ConsoleKeyInfo('w', ConsoleKey.W, false, false, false))
                .Returns(new ConsoleKeyInfo('t', ConsoleKey.T, false, false, false))
                .Returns(new ConsoleKeyInfo('f', ConsoleKey.F, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Enter, false, false, false));

            //Act
            var result = Instance.TryAskSecret(text, args);

            //Assert
            result.Should().Be("wtf");
        }

        [TestMethod]
        public void WhenUsingBackspace_DeleteLastCharacter()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            GetMock<IConsole>().SetupSequence(x => x.ReadAndInterceptKey())
                .Returns(new ConsoleKeyInfo('w', ConsoleKey.W, false, false, false))
                .Returns(new ConsoleKeyInfo('t', ConsoleKey.T, false, false, false))
                .Returns(new ConsoleKeyInfo('f', ConsoleKey.F, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Backspace, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Enter, false, false, false));

            //Act
            var result = Instance.TryAskSecret(text, args);

            //Assert
            result.Should().Be("wt");
        }

        [TestMethod]
        public void Always_IgnoreControlKeys()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.CreateMany<object>().ToArray();

            var textWithColor = text.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            GetMock<IConsole>().SetupSequence(x => x.ReadAndInterceptKey())
                .Returns(new ConsoleKeyInfo('w', ConsoleKey.W, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.LeftArrow, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Escape, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.UpArrow, false, false, false))
                .Returns(new ConsoleKeyInfo('t', ConsoleKey.T, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.DownArrow, false, false, false))
                .Returns(new ConsoleKeyInfo('f', ConsoleKey.F, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.RightArrow, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.PageUp, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.PageDown, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Enter, false, false, false));

            //Act
            var result = Instance.TryAskSecret(text, args);

            //Assert
            result.Should().Be("wtf");
        }
    }

    [TestClass]
    public class BreakLine : TerminalTesterBase
    {
        [TestMethod]
        public void Always_WriteAnEmptyLine()
        {
            //Arrange

            //Act
            Instance.BreakLine();

            //Assert
            GetMock<IConsole>().Verify(x => x.WriteLine(), Times.Once);
        }
    }

    [TestClass]
    public class WaitForAnyInput : TerminalTesterBase
    {
        [TestMethod]
        public void Always_WritePressAnyKeyMessage()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var textWithColor = Text.PressAnyKeyToContinue.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.WaitForAnyInput();

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void Always_ReadAndInterceptKey()
        {
            //Arrange
            var options = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            GetMock<IOptions<TerminalSettings>>().Setup(x => x.Value).Returns(options);
            var foregroundColor = options.Main!.Color!.Foreground!.Value;

            var textWithColor = Text.PressAnyKeyToContinue.Color(foregroundColor);
            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.WaitForAnyInput();

            //Assert
            GetMock<IConsole>().Verify(x => x.ReadAndInterceptKey(), Times.Once);
        }
    }

    [TestClass]
    public class AskForConfirmation : TerminalTesterBase
    {
        private TerminalSettings? _settings;

        protected override void InitializeTest()
        {
            base.InitializeTest();
            _settings = Fixture.Create<TerminalSettings>() with { TimeStamps = new TerminalSettings.TimeStampSettings { Use = false } };
            SetupOptions(_settings);
            var key = new ConsoleKeyInfo(Fixture.Create<char>(), new List<ConsoleKey> { YesKey, NoKey }.GetRandom(), false, false, false);
            GetMock<IConsole>().Setup(x => x.ReadAndInterceptKey()).Returns(key);
        }

        [TestMethod]
        public void Always_WriteQuestionFollowedByYesOrNo()
        {
            //Arrange
            var foregroundColor = _settings!.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.Create<object[]>();

            var textWithColor = $"{text} ({YesKey}/{NoKey})".Color(foregroundColor);
            var formattedText = textWithColor;
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            //Act
            Instance.AskForConfirmation(text, args);

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void WhenAnsweringYes_ReturnTrue()
        {
            //Arrange
            var foregroundColor = _settings!.Main!.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.Create<object[]>();

            var textWithColor = $"{text} ({YesKey}/{NoKey})".Color(foregroundColor);
            var formattedText = textWithColor;
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            GetMock<IConsole>().Setup(x => x.ReadAndInterceptKey()).Returns(new ConsoleKeyInfo(Fixture.Create<char>(), YesKey, false, false, false));

            //Act
            var result = Instance.AskForConfirmation(text, args);

            //Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenAnsweringNo_ReturnFalse()
        {
            //Arrange
            var foregroundColor = _settings!.Main.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.Create<object[]>();

            var textWithColor = $"{text} ({YesKey}/{NoKey})".Color(foregroundColor);
            var formattedText = textWithColor;
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            GetMock<IConsole>().Setup(x => x.ReadAndInterceptKey()).Returns(new ConsoleKeyInfo(Fixture.Create<char>(), NoKey, false, false, false));

            //Act
            var result = Instance.AskForConfirmation(text, args);

            //Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenAnsweringSomethingElseThanYesOrNo_AskAgainUntilAnswerIsYesOrNo()
        {
            //Arrange
            var foregroundColor = _settings!.Main.Color!.Foreground!.Value;

            var text = Fixture.Create<string>();
            var args = Fixture.Create<object[]>();

            var textWithColor = $"{text} ({YesKey}/{NoKey})".Color(foregroundColor);
            var formattedText = textWithColor;
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert(textWithColor)).Returns(formattedText);

            var invalidKeys = Enum.GetValues<ConsoleKey>().Where(x => x != YesKey && x != NoKey).ToList();

            var key = new ConsoleKeyInfo(Fixture.Create<char>(), new List<ConsoleKey> { YesKey, NoKey }.GetRandom(), false, false, false);

            GetMock<IConsole>().SetupSequence(x => x.ReadAndInterceptKey())
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), invalidKeys.GetRandom(), false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), invalidKeys.GetRandom(), false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), invalidKeys.GetRandom(), false, false, false))
                .Returns(key);

            //Act
            var result = Instance.AskForConfirmation(text, args);

            //Assert
            result.Should().Be(key.Key == YesKey);
        }
    }

    [TestClass]
    public class AskForLogin : TerminalTesterBase
    {
        [TestMethod]
        public void Always_WriteUsernameText()
        {
            //Arrange
            var formattedUsernameText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert($"{Text.Username} : ".Color(ForegroundColor))).Returns(formattedUsernameText);

            var formattedPasswordText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert($"{Text.Password} : ".Color(ForegroundColor))).Returns(formattedPasswordText);

            GetMock<IConsole>().Setup(x => x.ReadLine()).Returns(Fixture.Create<string>());

            GetMock<IConsole>().Setup(x => x.ReadAndInterceptKey()).Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Enter, false, false, false));

            //Act
            Instance.AskForLogin();

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedUsernameText), Times.Once);
        }

        [TestMethod]
        public void Always_WritePasswordText()
        {
            //Arrange
            var formattedUsernameText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert($"{Text.Username} : ".Color(ForegroundColor))).Returns(formattedUsernameText);

            var formattedPasswordText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert($"{Text.Password} : ".Color(ForegroundColor))).Returns(formattedPasswordText);

            GetMock<IConsole>().Setup(x => x.ReadLine()).Returns(Fixture.Create<string>());

            GetMock<IConsole>().Setup(x => x.ReadAndInterceptKey()).Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Enter, false, false, false));

            //Act
            Instance.AskForLogin();

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedPasswordText), Times.Once);
        }

        [TestMethod]
        public void WhenUsernameIsBlank_KeepAskingUntilGiven()
        {
            //Arrange
            var formattedUsernameText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert($"{Text.Username} : ".Color(ForegroundColor))).Returns(formattedUsernameText);

            var formattedPasswordText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert($"{Text.Password} : ".Color(ForegroundColor))).Returns(formattedPasswordText);

            var validResponse = Fixture.Create<string>();
            GetMock<IConsole>().SetupSequence(x => x.ReadLine()).Returns((string)null!).Returns("   ").Returns(string.Empty).Returns(validResponse);

            GetMock<IConsole>().Setup(x => x.ReadAndInterceptKey()).Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Enter, false, false, false));

            //Act
            var result = Instance.AskForLogin();

            //Assert
            result.Name.Should().Be(validResponse);
        }

        [TestMethod]
        public void WhenPasswordIsBlank_ReturnLoginInfo()
        {
            //Arrange
            var formattedUsernameText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert($"{Text.Username} : ".Color(ForegroundColor))).Returns(formattedUsernameText);

            var formattedPasswordText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert($"{Text.Password} : ".Color(ForegroundColor))).Returns(formattedPasswordText);

            GetMock<IConsole>().Setup(x => x.ReadLine()).Returns(Fixture.Create<string>());

            GetMock<IConsole>().Setup(x => x.ReadAndInterceptKey()).Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Enter, false, false, false));

            //Act
            var result = Instance.AskForLogin();

            //Assert
            result.Password.Should().BeEmpty();
        }

        [TestMethod]
        public void WhenPasswordIsNotBlank_ReturnLoginInfo()
        {
            //Arrange
            var formattedUsernameText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert($"{Text.Username} : ".Color(ForegroundColor))).Returns(formattedUsernameText);

            var formattedPasswordText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert($"{Text.Password} : ".Color(ForegroundColor))).Returns(formattedPasswordText);

            GetMock<IConsole>().Setup(x => x.ReadLine()).Returns(Fixture.Create<string>());

            GetMock<IConsole>().SetupSequence(x => x.ReadAndInterceptKey())
                .Returns(new ConsoleKeyInfo('H', ConsoleKey.H, false, false, false))
                .Returns(new ConsoleKeyInfo('e', ConsoleKey.E, false, false, false))
                .Returns(new ConsoleKeyInfo('l', ConsoleKey.L, false, false, false))
                .Returns(new ConsoleKeyInfo('l', ConsoleKey.L, false, false, false))
                .Returns(new ConsoleKeyInfo('o', ConsoleKey.O, false, false, false))
                .Returns(new ConsoleKeyInfo(Fixture.Create<char>(), ConsoleKey.Enter, false, false, false));

            //Act
            var result = Instance.AskForLogin();

            //Assert
            result.Password.Should().Be("Hello");
        }
    }

    [TestClass]
    public class AskChoice : TerminalTesterBase
    {
        [TestMethod]
        public void WhenChoicesAreNull_Throw()
        {
            //Arrange
            Choice[] choices = null!;

            //Act
            var action = () => Instance.AskChoice(choices);

            //Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenChoicesAreEmpty_Throw()
        {
            //Arrange
            var choices = Array.Empty<Choice>();

            //Act
            var action = () => Instance.AskChoice(choices);

            //Assert
            action.Should().Throw<ArgumentException>().WithMessage(Exceptions.NoChoice);
        }

        [TestMethod]
        public void WhenThereIsOneDuplicateChoice_Throw()
        {
            //Arrange
            var choices = Fixture.CreateMany<Choice>().ToList();
            var duplicateIdentifier = choices.GetRandom().Identifier;
            choices.Add(Fixture.Create<Choice>() with { Identifier = duplicateIdentifier });

            //Act
            var action = () => Instance.AskChoice(choices.ToArray());

            //Assert
            action.Should().Throw<ArgumentException>().WithMessage(string.Format(Exceptions.DuplicateIdentifiers, duplicateIdentifier));
        }

        [TestMethod]
        public void WhenThereAreManyDuplicateChoices_Throw()
        {
            //Arrange
            var choices = Fixture.CreateMany<Choice>().ToList();
            var duplicateIdentifiers = choices.Select(x => x.Identifier);
            choices = choices.Concat(choices).ToList();

            //Act
            var action = () => Instance.AskChoice(choices.ToArray());

            //Assert
            action.Should().Throw<ArgumentException>().WithMessage(string.Format(Exceptions.DuplicateIdentifiers, string.Join(',', duplicateIdentifiers)));
        }


        [TestMethod]
        public void WhenThereIsOnlyOneChoice_DisplayTheOneChoice()
        {
            //Arrange
            var choice = Fixture.Create<Choice>();

            GetMock<IConsole>().Setup(x => x.ReadLine()).Returns(choice.Identifier.ToLowerInvariant());

            var formattedText = Fixture.Create<string>();
            GetMock<IDmlAnsiConverter>().Setup(x => x.Convert($"{choice.Identifier}. {choice.Text}".Color(ForegroundColor))).Returns(formattedText);

            //Act
            Instance.AskChoice(choice);

            //Assert
            GetMock<IConsole>().Verify(x => x.Write(formattedText));
        }

        [TestMethod]
        public void WhenThereAreManyChoices_DisplayAllChoices()
        {
            //Arrange
            var choices = Fixture.CreateMany<Choice>().ToArray();

            GetMock<IConsole>().Setup(x => x.ReadLine()).Returns(choices.GetRandom().Identifier.ToLowerInvariant());

            var formattedTexts = new List<string>();
            foreach (var choice in choices)
            {
                var formattedText = Fixture.Create<string>();
                formattedTexts.Add(formattedText);
                GetMock<IDmlAnsiConverter>().Setup(x => x.Convert($"{choice.Identifier}. {choice.Text}".Color(ForegroundColor))).Returns(formattedText);
            }

            //Act
            Instance.AskChoice(choices);

            //Assert
            foreach (var text in formattedTexts)
                GetMock<IConsole>().Verify(x => x.Write(text), Times.Once);
        }

        [TestMethod]
        public void WhenOneChoiceInTheLotIsLongerThanOneCharacter_UseReadLineInsteadOfReadKey()
        {
            //Arrange
            var choices = new List<Choice>
            {
                new() { Identifier = "a", Text = Fixture.Create<string>(), Action = Fixture.Create<Action>()},
                new() { Identifier = "b", Text = Fixture.Create<string>(), Action = Fixture.Create<Action>()},
                new() { Identifier = "c", Text = Fixture.Create<string>(), Action = Fixture.Create<Action>()},
                Fixture.Create<Choice>()
            };

            GetMock<IConsole>().Setup(x => x.ReadLine()).Returns(choices.GetRandom().Identifier.ToLowerInvariant());
            GetMock<IConsole>().Setup(x => x.ReadKey()).Throws(Fixture.Create<Exception>());
            GetMock<IConsole>().Setup(x => x.ReadAndInterceptKey()).Throws(Fixture.Create<Exception>());

            var formattedTexts = new List<string>();
            foreach (var choice in choices)
            {
                var formattedText = Fixture.Create<string>();
                formattedTexts.Add(formattedText);
                GetMock<IDmlAnsiConverter>().Setup(x => x.Convert($"{choice.Identifier}. {choice.Text}".Color(ForegroundColor))).Returns(formattedText);
            }

            //Act
            Instance.AskChoice(choices.ToArray());

            //Assert
            foreach (var text in formattedTexts)
                GetMock<IConsole>().Verify(x => x.Write(text), Times.Once);
        }

        [TestMethod]
        public void WhenAllChoicesAreOneCharacterLong_UseReadKey()
        {
            //Arrange
            var choices = new List<Choice>
            {
                new() { Identifier = "a", Text = Fixture.Create<string>(), Action = Fixture.Create<Action>()},
                new() { Identifier = "b", Text = Fixture.Create<string>(), Action = Fixture.Create<Action>()},
                new() { Identifier = "c", Text = Fixture.Create<string>(), Action = Fixture.Create<Action>()},
            };

            GetMock<IConsole>().Setup(x => x.ReadLine()).Throws(Fixture.Create<Exception>());
            GetMock<IConsole>().Setup(x => x.ReadKey()).Returns(new ConsoleKeyInfo(choices.GetRandom().Identifier.First(), Fixture.Create<ConsoleKey>(), Fixture.Create<bool>(), Fixture.Create<bool>(), Fixture.Create<bool>()));

            var formattedTexts = new List<string>();
            foreach (var choice in choices)
            {
                var formattedText = Fixture.Create<string>();
                formattedTexts.Add(formattedText);
                GetMock<IDmlAnsiConverter>().Setup(x => x.Convert($"{choice.Identifier}. {choice.Text}".Color(ForegroundColor))).Returns(formattedText);
            }

            //Act
            Instance.AskChoice(choices.ToArray());

            //Assert
            foreach (var text in formattedTexts)
                GetMock<IConsole>().Verify(x => x.Write(text), Times.Once);
        }

        [TestMethod]
        public void WhenMakingChoice_ExecuteAssociatedAction()
        {
            //Arrange
            var choices = Fixture.CreateMany<Choice>().ToList();
            var isExecuted = false;
            choices.Add(Fixture.Build<Choice>().With(x => x.Action, () => isExecuted = true).Create());

            GetMock<IConsole>().Setup(x => x.ReadLine()).Returns(choices.Last().Identifier.ToLowerInvariant());
            
            foreach (var choice in choices)
            {
                var formattedText = Fixture.Create<string>();
                GetMock<IDmlAnsiConverter>().Setup(x => x.Convert($"{choice.Identifier}. {choice.Text}".Color(ForegroundColor))).Returns(formattedText);
            }

            //Act
            Instance.AskChoice(choices.ToArray());

            //Assert
            isExecuted.Should().BeTrue();
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow(null)]
        public void WhenThereAreEmptyIdentifiers_DoNotThrow(string identifier)
        {
            //Arrange
            var choices = Fixture.Build<Choice>().With(x => x.Identifier, identifier).CreateMany().ToList();

            var identifiers = new List<string> { "1", "2", "3" };

            GetMock<IConsole>().Setup(x => x.ReadKey()).Returns(new ConsoleKeyInfo(identifiers.GetRandom().First(), Fixture.Create<ConsoleKey>(), Fixture.Create<bool>(), Fixture.Create<bool>(), Fixture.Create<bool>()));

            foreach (var choice in choices)
            {
                var formattedText = Fixture.Create<string>();
                GetMock<IDmlAnsiConverter>().Setup(x => x.Convert($"{choice.Identifier}. {choice.Text}".Color(ForegroundColor))).Returns(formattedText);
            }

            //Act
            var action = () => Instance.AskChoice(choices.ToArray());

            //Assert
            action.Should().NotThrow();
        }
    }
}
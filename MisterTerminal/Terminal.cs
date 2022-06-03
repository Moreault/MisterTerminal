namespace ToolBX.MisterTerminal;

public interface ITerminal : ITerminalWriter
{
    IDebugTerminal Debug { get; }
    INotificationTerminal Notification { get; }
    IErrorTerminal Error { get; }
    IWarningTerminal Warning { get; }

    /// <summary>
    /// Asks a question to the user until they give any response.
    /// </summary>
    string Ask(string text, params object[]? args);

    /// <summary>
    /// Asks a question to the user while also accepting no answer and ignoring exceptions.
    /// </summary>
    string TryAsk(string text, params object[]? args);

    /// <summary>
    /// Asks a question to the user on the same line until they give any response.
    /// </summary>
    string AskOnSameLine(string text, params object[]? args);

    /// <summary>
    /// Asks a question to the user on the same line while also accepting no answer and ignoring exceptions.
    /// </summary>
    string TryAskOnSameLine(string text, params object[]? args);

    /// <summary>
    /// Asks a question to the user while hiding the response.
    /// </summary>
    string AskSecret(string text, params object[]? args);

    /// <summary>
    /// Asks a question to the user while hiding the response while also accepting no answer and ignoring exceptions.
    /// </summary>
    string TryAskSecret(string text, params object[]? args);

    /// <summary>
    /// Adds a blank line.
    /// </summary>
    void BreakLine();

    /// <summary>
    /// Press any key to continue...
    /// </summary>
    void WaitForAnyInput();

    /// <summary>
    /// Asks the user a question until they answer yes or no.
    /// </summary>
    bool AskForConfirmation(string text, params object[]? args);

    /// <summary>
    /// Asks user to provide a username and password.
    /// </summary>
    UserLoginInfo AskForLogin();

    void AskChoice(params Choice[] choices);

}

[AutoInject]
public class Terminal : TerminalWriter, ITerminal
{
    public IDebugTerminal Debug { get; }
    public INotificationTerminal Notification { get; }
    public IErrorTerminal Error { get; }
    public IWarningTerminal Warning { get; }

    private readonly IConsole _console;
    private readonly TerminalSettings _settings;

    private readonly ConsoleKey _yesKey;
    private readonly ConsoleKey _noKey;

    public Terminal(IConsole console, IOptions<TerminalSettings> settings, IDebugTerminal debugTerminal, IDmlAnsiConverter dmlAnsiConverter, IWarningTerminal warningTerminal, IErrorTerminal errorTerminal, INotificationTerminal notificationTerminal) : base(console, settings, dmlAnsiConverter)
    {
        _console = console;
        _settings = settings.Value;

        DefaultBackgroundColor = settings.Value.Main?.Color?.Background ?? DefaultColors.Main.Background!.Value;
        DefaultForegroundColor = settings.Value.Main?.Color?.Foreground ?? DefaultColors.Main.Foreground!.Value;

        Debug = debugTerminal;
        Warning = warningTerminal;
        Error = errorTerminal;
        Notification = notificationTerminal;

        _yesKey = (ConsoleKey)Text.Yes.ToUpperInvariant()[0];
        _noKey = (ConsoleKey)Text.No.ToUpperInvariant()[0];
    }

    public string Ask(string text, params object[]? args)
    {
        Write(text, args);
        return WaitResponse();
    }

    public string AskOnSameLine(string text, params object[]? args)
    {
        WriteWithoutBreakingLine(text, args);
        return WaitResponse();
    }

    private string WaitResponse()
    {
        var response = string.Empty;
        while (string.IsNullOrWhiteSpace(response))
            response = _console.ReadLine();
        return response;
    }

    public string TryAsk(string text, params object[]? args)
    {
        TryWrite(text, args);
        return _console.ReadLine() ?? string.Empty;
    }

    public string TryAskOnSameLine(string text, params object[]? args)
    {
        if (!string.IsNullOrWhiteSpace(text))
            WriteWithoutBreakingLine(text, args);
        return _console.ReadLine() ?? string.Empty;
    }

    public string AskSecret(string text, params object[]? args)
    {
        WriteWithoutBreakingLine(text, args);
        var response = WaitForSecretResponse();
        BreakLine();
        return response;
    }

    public string TryAskSecret(string text, params object[]? args)
    {
        if (!string.IsNullOrWhiteSpace(text))
            WriteWithoutBreakingLine(text, args);

        var response = WaitForSecretResponse();
        BreakLine();
        return response;
    }

    private string WaitForSecretResponse()
    {
        var response = string.Empty;
        while (true)
        {
            var key = _console.ReadAndInterceptKey();
            if (key.Key == ConsoleKey.Enter) break;
            if (key.Key == ConsoleKey.Backspace)
            {
                if (response.Length > 0)
                    response = response[..^1];
                continue;
            }

            if (char.IsControl(key.KeyChar) ||
                key.Key == ConsoleKey.DownArrow ||
                key.Key == ConsoleKey.UpArrow ||
                key.Key == ConsoleKey.LeftArrow ||
                key.Key == ConsoleKey.RightArrow ||
                key.Key == ConsoleKey.Escape ||
                key.Key == ConsoleKey.PageUp ||
                key.Key == ConsoleKey.PageDown) continue;
            response += key.KeyChar;
        }

        return response;
    }

    public void BreakLine() => _console.WriteLine();

    public void WaitForAnyInput()
    {
        Write(Text.PressAnyKeyToContinue);
        _console.ReadAndInterceptKey();
    }

    public bool AskForConfirmation(string text, params object[]? args)
    {
        Write($"{text} ({_yesKey}/{_noKey})", args);

        var response = new ConsoleKeyInfo();
        while (response.Key != _yesKey && response.Key != _noKey)
            response = _console.ReadAndInterceptKey();

        return response.Key == _yesKey;
    }

    public UserLoginInfo AskForLogin()
    {
        var username = AskOnSameLine($"{Text.Username} : ");
        var password = AskSecret($"{Text.Password} : ");

        return new UserLoginInfo(username, password);
    }

    private ConsoleKeyInfo ReadKeyAndBreakLine()
    {
        var key = _console.ReadKey();
        BreakLine();
        return key;
    }

    public void AskChoice(params Choice[] choices)
    {
        if (choices == null) throw new ArgumentNullException(nameof(choices));
        if (!choices.Any()) throw new ArgumentException(Exceptions.NoChoice);
        var duplicates = choices.Select(x => x.Identifier.ToLowerInvariant()).GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
        if (duplicates.Any())
            throw new ArgumentException(string.Format(Exceptions.DuplicateIdentifiers, string.Join(',', duplicates)));

        var i = 1;

        var finalChoices = new List<Choice>();
        foreach (var option in choices)
        {
            var identifier = string.IsNullOrWhiteSpace(option.Identifier) ? i++.ToString() : option.Identifier;

            Write($"{identifier}. {option.Text}");
            finalChoices.Add(option with { Identifier = identifier });
        }

        Choice? choice = null;

        var isSingleKey = finalChoices.All(x => x.Identifier.Length == 1);

        while (choice == null)
        {
            var identifier = isSingleKey ? ReadKeyAndBreakLine().KeyChar.ToString() : _console.ReadLine();
            choice = finalChoices.SingleOrDefault(x => string.Equals(x.Identifier, identifier, StringComparison.InvariantCultureIgnoreCase));
        }

        choice.Action.Invoke();
    }
}
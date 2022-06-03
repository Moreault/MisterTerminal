namespace MisterTerminal.Sample;

public interface ISampleConsole
{
    void Start();
}

[AutoInject]
public class SampleConsole : ISampleConsole
{
    private readonly ITerminal _terminal;

    public SampleConsole(ITerminal terminal)
    {
        _terminal = terminal;
    }

    public void Start()
    {
        _terminal.Debug.Write("This is debug stuff and also appears both in the regular AND debug consoles.");
        _terminal.Write("<color red=200 blue=150><underline>This</underline> uses</color> <bold><color red=125 green=255 blue=0>DML</color></bold> to output <italic>styles</italic> and <strikeout><color red=255>color</color></strikeout>.");

        _terminal.Notification.Write("We will never ask you for your username or password");
        var logininfo = _terminal.AskForLogin();

        _terminal.Write($"Good morning, {logininfo.Name}");

        if (logininfo.Password.Length < 4)
            _terminal.Warning.Write("Your password appears to be weaksauce.");

        _terminal.Write("What do you want to test?");
        _terminal.AskChoice(new Choice("Quest", () => { _terminal.Write("Quest is not yet implemented :("); }),
            new Choice("Confirmation", () => { _terminal.Write("Confirmation is not yet implemented :("); }),
            new Choice("Quit", () => { _terminal.Write("Quitting is not yet implemented :("); }));


        _terminal.Error.Write("Critical error : It appears that this application only does this!");

        var isItOk = _terminal.AskForConfirmation("Is it okay?");
        _terminal.Write(isItOk ? "Phew. Bye now!" : "Oh... well... too bad. Bye!");

        _terminal.WaitForAnyInput();
    }
}
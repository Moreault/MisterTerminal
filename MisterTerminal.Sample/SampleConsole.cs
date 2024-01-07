namespace MisterTerminal.Sample;

public interface ISampleConsole
{
    void Start();
}

[AutoInject(ServiceLifetime.Singleton)]
public class SampleConsole(ITerminal terminal) : ISampleConsole
{
    public void Start()
    {
        terminal.Debug.Write("This is debug stuff and also appears both in the regular AND debug consoles.");
        terminal.Write("<color red=200 blue=150><underline>This</underline> uses</color> <bold><color red=125 green=255 blue=0>DML</color></bold> to output <italic>styles</italic> and <strikeout><color red=255>color</color></strikeout>.");

        terminal.Notification.Write("We will never ask you for your username or password");
        var logininfo = terminal.AskForLogin();

        terminal.Write($"Good morning, {logininfo.Name}");

        if (logininfo.Password.Length < 4)
            terminal.Warning.Write("Your password appears to be weaksauce.");

        terminal.Write("What do you want to test?");
        terminal.AskChoice(new Choice("Quest", () => { terminal.Write("Quest is not yet implemented :("); }),
            new Choice("Confirmation", () => { terminal.Write("Confirmation is not yet implemented :("); }),
            new Choice("Quit", () => { terminal.Write("Quitting is not yet implemented :("); }));


        terminal.Error.Write("Critical error : It appears that this application only does this!");

        var isItOk = terminal.AskForConfirmation("Is it okay?");
        terminal.Write(isItOk ? "Phew. Bye now!" : "Oh... well... too bad. Bye!");

        terminal.WaitForAnyInput();
    }
}
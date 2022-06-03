namespace ToolBX.MisterTerminal;

public interface IDmlAnsiConverter
{
    string Convert(string text);
}

[AutoInject]
public class DmlAnsiConverter : IDmlAnsiConverter
{
    private readonly IDmlSerializer _dmlSerializer;

    //https://no-color.org/
    private readonly bool _isUsingNoColor;

    public DmlAnsiConverter(IDmlSerializer dmlSerializer, IEnvironmentVariables environmentVariables)
    {
        _dmlSerializer = dmlSerializer;
        _isUsingNoColor = environmentVariables.Contains(EnvironmentVariableNames.NoColor);
    }

    public string Convert(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentNullException(nameof(text));
        var dmlString = _dmlSerializer.Deserialize(text);
        var formattedText = string.Empty;
        foreach (var dmlTag in dmlString)
        {
            var piece = dmlTag.Text;
            if (dmlTag.Color.HasValue && !_isUsingNoColor)
                piece = $"{dmlTag.Color.Value.ToAnsiTrueColor()}{piece}\u001b[0m";
            if (dmlTag.Styles.Any(x => x == TextStyle.Bold))
                piece = $"\u001b[1m{piece}\u001b[0m";
            if (dmlTag.Styles.Any(x => x == TextStyle.Italic))
                piece = $"\u001b[3m{piece}\u001b[0m";
            if (dmlTag.Styles.Any(x => x == TextStyle.Underline))
                piece = $"\u001b[4m{piece}\u001b[0m";
            if (dmlTag.Styles.Any(x => x == TextStyle.Strikeout))
                piece = $"\u001b[9m{piece}\u001b[0m";

            formattedText += piece;
        }

        return formattedText;
    }
}
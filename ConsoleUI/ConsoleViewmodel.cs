namespace ConsoleUI;

internal class ConsoleViewmodel
{
    public event Action<string>? NewNotification;
    public event Action<string>? ConnectionChanged;

    public event Action<string>? NewTrade;
    public event Action<string>? NewLogMessage;
    public event Action<string[]>? SecuritiesChanged;

    public event Action<Func<string>>? UserPromptRequested;
}

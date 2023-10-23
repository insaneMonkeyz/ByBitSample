namespace ConsoleUI
{
    internal static class ConsoleUI
    {
        private const string Template =
            $$"""
            
            {0}
            
            =======================================================================
            """;

        private static string CommandsDescriptionMessage
        {
            get => string.Format(Template, CommandsDescriptionContent);
        }

        private const string CommandsDescriptionContent =
            $$"""
                    Press Ctrl+C to connect to the server
                    Press Ctrl+D to disconnect from the server

                    Press Ctrl+S to subscribe to trades
                    Press Ctrl+U to unsubscribe from trades

                    Press Ctrl+A to output available securities
                    Press Ctrl+T to output subscribed trades
                    Press Ctrl+L to output log
            """;

        private static string SubscriptionsMessage 
        { 
            get => string.Format(Template, $"\t{SubscriptionsContent}"); 
        }
        private static string SubscriptionsContent
        {
            get
            {
                return _subscriptions.Count > 0
                    ? string.Join(Environment.NewLine + "\t", _subscriptions)
                    : "No subscriptions yet";
            }
        }

        private static string PromptTemplate
        {
            get => string.Format(Template,"\t");
        }

        private static int CommandsTemplatePosition => 0;
        private static int SubscribtionsTemplatePosition 
        { 
            get => CommandsTemplatePosition + CommandsDescriptionMessage.Count(c => c == '\n') + 1; 
        }
        private static int PromptTemplatePosition
        {
            get => SubscribtionsTemplatePosition + SubscriptionsMessage.Count(c => c == '\n') + 1;
        }
        private static int PromptPosition
        {
            get => PromptTemplatePosition + 1;
        }
        private static int OutputPosition 
        { 
            get => PromptTemplatePosition + PromptTemplate.Count(c => c == '\n') + 1; 
        }

        public static void Initialize(ConsoleViewmodel vm)
        {
            _viewmodel = vm;
            SetConsoleSizeWatcher();
            PlotCommandsDescription();
            PlotSubscriptions();
        }
        public static string? PromptUser(string question)
        {
            return PlotUserPrompt(question);
        }

        private static void PlotCommandsDescription()
        {

            lock (_outputSync)
            {
                Console.SetCursorPosition(0, 0);
                Console.Write(CommandsDescriptionMessage);
                Console.SetCursorPosition(0, SubscribtionsTemplatePosition); 
            }
        }
        private static void PlotSubscriptions()
        {
            const string NoSubscribtionsMessage = "\tNo subscriptions";

            lock (_outputSync)
            {
                Console.SetCursorPosition(0, SubscribtionsTemplatePosition);
                Console.Write(SubscriptionsMessage);
                Console.SetCursorPosition(0, OutputPosition);
            }
        }
        private static string? PlotUserPrompt(string message)
        {
            lock (_outputSync)
            {
                Console.SetCursorPosition(0, PromptTemplatePosition);
                Console.Write(PromptTemplate);
                Console.SetCursorPosition(0, PromptPosition);
                Console.Write($"\t{message}");
                return Console.ReadLine();
            }
        }

        private static void SetConsoleSizeWatcher()
        {
            const int MinWidth = 75;
            const int MinHeight = 36;

            Task.Run(() =>
            {
                while (true)
                {
                    if (Console.WindowHeight > MinHeight || Console.WindowWidth > MinWidth)
                    {
                        Console.SetWindowSize(MinWidth, MinHeight);
                    }
                    Thread.Sleep(30);
                }
            });
        }

        private static ConsoleViewmodel _viewmodel;
        private static readonly object _outputSync = new();
        private static readonly List<string> _subscriptions = new();
        private static readonly List<string> _available = new(1000);
        private static readonly Queue<string> _logQueue = new(10000);
        private static readonly Queue<string> _tradesQueue = new(10000);
    }
}

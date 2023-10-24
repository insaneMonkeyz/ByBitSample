using System.Text;

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
            get => string.Format(Template, _viewmodel.CommandsDescriptionContent);
        }
        private static string SubscriptionsMessage 
        { 
            get => string.Format(Template, $"\t{_viewmodel.SubscriptionsContent}"); 
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
            if (_initialized)
            {
                return;
            }

            CommandsManager.HopOnTopCommandRequested += OnHopOnTopCommandRequested;
            Console.SetWindowSize(ConsoleMinWidth, ConsoleMinHeight);

            _initialized = true;
            _viewmodel = vm;

            Console.Title = vm.ConnectionState;

            vm.ConnectionChanged += state => Console.Title = state;
            vm.NewNotification += PlotMessage;
            vm.NewContent += AppendContent;
            vm.ContentKindChanged += Redraw;
            vm.UserPromptRequested += OnUserPromptRequested;
            vm.SubscriptionChanged += OnSubscriptionChanged;

            Redraw();
            SetConsoleSizeWatcher();
        }

        private static void OnHopOnTopCommandRequested()
        {
            lock (_outputSync)
            {
                Console.SetCursorPosition(0, 0);
                Console.SetCursorPosition(0, PromptPosition);
            }
        }

        private static void OnSubscriptionChanged()
        {
            Redraw();
        }
        private static void OnUserPromptRequested(string message, Action<string> inputHandler)
        {
            inputHandler(PlotUserPrompt(message));
        }

        private static void PlotCommandsDescription()
        {

            lock (_outputSync)
            {
                Console.SetCursorPosition(0, 0);
                Console.Write(CommandsDescriptionMessage);
                SetCursorToLastPosition();
            }
        }
        private static void PlotSubscriptions()
        {
            const string NoSubscribtionsMessage = "\tNo subscriptions";

            lock (_outputSync)
            {
                var msg = SubscriptionsMessage;
                var numLines = msg.Count(c => c == '\n');

                ClearRange(SubscribtionsTemplatePosition, numLines);
                Console.SetCursorPosition(0, SubscribtionsTemplatePosition);
                Console.Write(msg);
                SetCursorToLastPosition();
            }
        }
        private static void PlotContent()
        {
            if (!_viewmodel.Content.Any())
            {
                return;
            }

            var sb = new StringBuilder(65_536);

            sb.Append('\t');
            sb.AppendJoin("\n\t", _viewmodel.Content);

            _contentLength = _viewmodel.Content.TryGetNonEnumeratedCount(out int count)
                ? count
                : _viewmodel.Content.Count();

            lock (_outputSync)
            {
                Console.SetCursorPosition(0, OutputPosition + 1);
                Console.WriteLine(sb.ToString());
            }
        }
        private static string? PlotUserPrompt(string message)
        {
            lock (_outputSync)
            {
                PlotMessage(message);
                var input = Console.ReadLine();
                SetCursorToLastPosition();
                return input;
            }
        }
        private static void PlotMessage(string message)
        {
            lock (_outputSync)
            {
                Console.SetCursorPosition(0, PromptTemplatePosition);
                Console.Write(PromptTemplate);

                Console.SetCursorPosition(0, PromptPosition);
                Console.Write(_emptyLine);

                Console.SetCursorPosition(0, PromptPosition);
                Console.Write($"\t{message}");
            }
        }
        private static void AppendContent(string message)
        {
            lock (_outputSync)
            {
                Console.SetCursorPosition(0, OutputPosition + _contentLength++);
                Console.WriteLine(message);
            }
        }
        private static void RedrawServicePart()
        {
            lock (_outputSync)
            {
                PlotCommandsDescription();
                PlotSubscriptions();
                PlotMessage(string.Empty); 
            }
        }
        private static void Redraw()
        {
            lock (_outputSync)
            {
                Console.Clear();
                RedrawServicePart();
                PlotContent(); 
            }
        }
        private static void SetCursorToLastPosition()
        {
            lock( _outputSync)
            {
                Console.SetCursorPosition(0, OutputPosition + _contentLength + 1);
            }
        }
        private static void ClearRange(int position, int depth)
        {
            lock (_outputSync)
            {
                Console.SetCursorPosition(0, position);

                for (int i = 0; i < depth; i++)
                {
                    Console.WriteLine(_emptyLine);
                }
            }
        }
        private static void SetConsoleSizeWatcher()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        if (Console.WindowHeight < ConsoleMinHeight || Console.WindowWidth < ConsoleMinWidth)
                        {
                            Console.SetWindowSize(ConsoleMinWidth, ConsoleMinHeight);
                        }
                        if (Console.WindowWidth != _lastConsoleWidth)
                        {
                            _lastConsoleWidth = Console.WindowWidth;
                            _emptyLine = new(' ', _lastConsoleWidth);
                        }
                        Task.Delay(50).Wait();
                    }
                    catch { }
                }
            });
        }

        private const int ConsoleMinWidth = 75;
        private const int ConsoleMinHeight = 36;

        private static int _lastConsoleWidth = 0;
        private static string? _emptyLine;
        private static int _contentLength;
        private static bool _initialized;
        private static ConsoleViewmodel _viewmodel;
        private static readonly object _outputSync = new();
    }
}

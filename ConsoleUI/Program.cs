using MarketDataProvider;
using ZeroLog;
using ZeroLog.Appenders;
using ZeroLog.Configuration;
using ZeroLog.Formatting;

namespace ConsoleUI;

internal class Program
{
    private static async Task Main(string[] args)
    {
        ConfigureLoggers();
        ConsoleUI.Initialize(new());
        await Task.Delay(Timeout.Infinite);
    }

    private static void ConfigureLoggers()
    {
        static AppenderConfiguration CreateDefaultLogTarget(string logfilename)
        {
            var logfile = $"{DateTime.Now:yyyyMMdd} {logfilename}.txt";
            var logstreamWriter = new StreamWriter(logfile, append: true);

            return new TextWriterAppender(logstreamWriter)
            {
                Formatter = new DefaultFormatter
                {
                    PrefixPattern = "[%date  %time] [%level] [%thread] [%loggerCompact] "
                }
            };
        }

        var logConfig = new ZeroLogConfiguration
        {
            RootLogger =
            {
                LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.Allocate,
                Level = LogLevel.Debug,
                Appenders =
                {
                    CreateDefaultLogTarget("general")
                }
            },
            Loggers =
            {
                new LoggerConfiguration(nameof(IDataTransmitter))
                {
                    LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.Allocate,
                    Level = LogLevel.Info,
                    Appenders =
                    {
                        CreateDefaultLogTarget("messages")
                    }
                },
                new LoggerConfiguration(nameof(IConnection))
                {
                    LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.Allocate,
                    Level = LogLevel.Debug,
                    Appenders =
                    {
                        CreateDefaultLogTarget("connection")
                    }
                }
            },
            AppendingStrategy = AppendingStrategy.Synchronous,
            AutoRegisterEnums = true,
            LogMessageBufferSize = 1024,
        };
        
        logConfig.ApplyChanges();
        LogManager.Initialize(logConfig);
    }
}
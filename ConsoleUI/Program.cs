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

            var logstream = new FileStream(logfile, FileMode.OpenOrCreate, FileAccess.Write);
            var logstreamWriter = new StreamWriter(logstream);

            return new TextWriterAppender(logstreamWriter)
            {
                Formatter = new DefaultFormatter
                {
                    PrefixPattern = "\n[%date  %time]\t[%level]\t[%thread]\t%[loggerCompact] "
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
                    CreateDefaultLogTarget("connection")
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
                }
            },
            AutoRegisterEnums = true,
            LogMessageBufferSize = 1024,
        };
        
        logConfig.ApplyChanges();
        LogManager.Initialize(logConfig);
    }
}
internal static class CommandsManager
{
    public static event Action? SubscribeCommandRequested;
    public static event Action? UnsubscribeCommandRequested;

    public static event Action? ConnectCommandRequested;
    public static event Action? DisconnectCommandRequested;

    public static event Action? ShowTradesCommandRequested;
    public static event Action? ShowLogCommandRequested;
    public static event Action? ShowSecuritiesCommandRequested;

    public static async Task ListenUserCommands()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                if (IsCtrlCombinationPressed(out ConsoleKey key))
                {
                    switch (key)
                    {
                        case ConsoleKey.S:
                            {
                                SafeInvoke(SubscribeCommandRequested);
                                break;
                            }
                        case ConsoleKey.U:
                            {
                                SafeInvoke(UnsubscribeCommandRequested);
                                break;
                            }
                        case ConsoleKey.K:
                            {
                                SafeInvoke(ConnectCommandRequested);
                                break;
                            }
                        case ConsoleKey.D:
                            {
                                SafeInvoke(DisconnectCommandRequested);
                                break;
                            }
                        case ConsoleKey.T:
                            {
                                SafeInvoke(ShowTradesCommandRequested);
                                break;
                            }
                        case ConsoleKey.L:
                            {
                                SafeInvoke(ShowLogCommandRequested);
                                break;
                            }
                        case ConsoleKey.E:
                            {
                                SafeInvoke(ShowSecuritiesCommandRequested);
                                break;
                            }
                    }
                }

                await Task.Delay(50);
            }
        });
    }
    private static void SafeInvoke(Action? action)
    {
        try
        {
            action?.Invoke();
        }
        catch { }
    }
    private static bool IsCtrlCombinationPressed(out ConsoleKey key)
    {
        key = default;

        if (!Console.KeyAvailable)
        {
            return false;
        }

        var pressedKeyInfo = Console.ReadKey(true);

        if ((pressedKeyInfo.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
        {
            key = pressedKeyInfo.Key;
            return true;
        }

        return false;
    }
}
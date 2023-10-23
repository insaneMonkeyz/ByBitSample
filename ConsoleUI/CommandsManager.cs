internal static class CommandsManager
{
    public static event Action SubscribeCommandRequested;
    public static event Action UnsubscribeCommandRequested;
    public static event Action ConnectCommandRequested;
    public static event Action DisconnectCommandRequested;

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
                                SubscribeCommandRequested();
                                break;
                            }
                        case ConsoleKey.U:
                            {
                                UnsubscribeCommandRequested();
                                break;
                            }
                        case ConsoleKey.C:
                            {
                                ConnectCommandRequested();
                                break;
                            }
                        case ConsoleKey.D:
                            {
                                DisconnectCommandRequested();
                                break;
                            }
                    }
                }

                await Task.Delay(50);
            }
        });
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
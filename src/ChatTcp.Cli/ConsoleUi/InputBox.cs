namespace ChatTcp.Cli.ConsoleUi;

internal class InputBox
{
    public static void Activate((Point start, Point end) inputArea, Action<string> submit)
    {
        var start = inputArea.start;
        var end = inputArea.end;

        SetCursorPosition(start);

        var maxlength = end.X - start.X;
        var inputString = string.Empty;
        var keyInfo = Console.ReadKey(true);

        while (true)
        {
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                submit(inputString);

                SetCursorPosition(start);

                for (int i = 0; i < inputString.Length; i++)
                {
                    Console.Write(" ");
                }

                SetCursorPosition(start);
                break;
            }
            //Erasing text (backspace)
            if (keyInfo.Key == ConsoleKey.Backspace && inputString.Length > 0)
            {
                inputString = inputString.Remove(inputString.Length - 1);
                Console.CursorLeft--;
                Console.Write(" ");
                Console.CursorLeft--;
                keyInfo = Console.ReadKey(true);
                continue;
            }

            //Disabled keys
            if (keyInfo.Key == ConsoleKey.Tab)
            {
                keyInfo = Console.ReadKey(true);
                continue;
            }
            //Writes to console if not maxlength
            //TODO: wrap around
            if (inputString.Length < maxlength)
            {
                inputString += keyInfo.KeyChar;
                Console.Write(keyInfo.KeyChar);
            }
            //All other cases wait for new key
            keyInfo = Console.ReadKey(true);
        }
    }

    private static void SetCursorPosition(Point point)
    {
        Console.SetCursorPosition(point.X, point.Y);
    }

}

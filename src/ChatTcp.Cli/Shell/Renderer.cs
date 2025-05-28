

using System.Net.Mail;

namespace ChatTcp.Cli.ConsoleUi;

internal class Renderer
{

    private char[,] _currentBuffer = new char[ShellSettings.FrameBufferWidth, ShellSettings.FrameBufferHeight];
    private char[,] _frameBuffer = new char[ShellSettings.FrameBufferWidth, ShellSettings.FrameBufferHeight];


    private Func<int> GetWindowWidth = () => Console.WindowWidth;
    private Func<int> GetWindowHeight = () => Console.WindowHeight;

    public async Task Start(AppState appState, CancellationToken cancellatioToken)
    {
        try
        {
            Console.CursorVisible = false;

            var windowWidth = -1;
            var windowHeight = -1;

            while (true)
            {
                if (windowWidth != GetWindowWidth() || windowHeight != GetWindowHeight())
                {
                    RefreshScreen();
                    windowWidth = GetWindowWidth();
                    windowHeight = GetWindowHeight();
                }

                SyncChatMessages(appState.Messages);
                SyncInputBuffer(appState.InputBuffer, windowWidth, windowHeight);
                SyncCursor(appState.CursorIndex, appState.InputBuffer, windowHeight);

                cancellatioToken.ThrowIfCancellationRequested();
                Render(windowWidth, windowHeight);

                await Task.Delay(ShellSettings.RefreshRate, cancellatioToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Rendering canceled...");
        }
        finally
        {
            Console.CursorVisible = true;
        }
    }

    private void RefreshScreen()
    {
        if (GetWindowHeight() > 0 && GetWindowWidth() > 0)
        {
            try
            {
                Console.Clear();
            }
            catch (IOException ex)
            {
                if (!ex.Message.Contains("The parameter is incorrect"))
                    throw;
            }
        }
        for (int x = 0; x < ShellSettings.FrameBufferWidth; x++)
        {
            for (int y = 0; y < ShellSettings.FrameBufferHeight; y++)
            {
                _currentBuffer[x, y] = '\0';
                _frameBuffer[x, y] = '\0';
            }
        }
    }

    private void SyncCursor(int cursorIndex, string inputBuffer, int windowHeight)
    {
        if (cursorIndex < 0 || cursorIndex > inputBuffer.Length)
        {
            throw new ShellException($"Cursor index must be within the input buffer length or one place outside. CursorIndex: {cursorIndex}, bufferLength: {inputBuffer.Length}, inputBuffer: {inputBuffer}");
        }

        var x = ShellSettings.Prompt.Length + cursorIndex + 1;
        var y = windowHeight < ShellSettings.PromptHeight ? windowHeight : windowHeight - ShellSettings.PromptHeight;

        _frameBuffer[x, y] = ShellSettings.Cursor;
    }

    private void SyncInputBuffer(string inputBuffer, int windowWidth, int windowHeight)
    {
        var prompt = $"{ShellSettings.Prompt} {inputBuffer}";
        var maxWidth = ShellSettings.FrameBufferWidth < (windowWidth - 1) ? ShellSettings.FrameBufferWidth : (windowWidth - 1);

        //just to not break the program on small windows
        if (windowHeight < ShellSettings.PromptHeight)
            return;

        for (int x = 0; x < maxWidth; x++)
        {
            var y = windowHeight - ShellSettings.PromptHeight;
            if (x < prompt.Length)
            {
                _frameBuffer[x, y] = prompt[x];
                continue;
            }

            //flush deleted chars
            _frameBuffer[x, y] = ' ';
        }
    }

    public static int CalculateRows(string text, int width)
    {
        int rows = 1;
        int length = text.Length;

        checked
        {
            while (length - width > 0)
            {
                rows++;
                length -= width;
            }
        }
        return rows;
    }

    public static int CalculateTotalChatRows(string[] messages, int width, int indentation)
    {
        int rows = 0;
        int messageWidth = width - indentation;

        foreach (var m in messages)
        {
            rows += CalculateRows(m, messageWidth);
        }

        return rows;
    }

    public static int FillTextFrame(char[,] frame, string text, int x0, int y0, int x1)
    {
        int x = x0;
        int y = y0;
        int chrPtr = 0;
        try
        {
            for (chrPtr = 0; chrPtr < text.Length; chrPtr++)
            {
                frame[x, y] = text[chrPtr];
                x++;
                if (x == x1)
                {
                    y++;
                    x = x0;
                }
            }
        }
        catch (IndexOutOfRangeException ex)
        {
            throw new ShellException($"Error inserting char '{text[chrPtr]}' in '{text}' index {chrPtr} at x: {x} y: {y}", ex);
        }

        return y + 1;
    }

    public static void FillChatFrame(int width, int height, char[,] frame, List<ChatMessage> messages)
    {
        //calculate rows to know if all messages fit in the chat window or if only the latest should be viewed
        int textMarginOverflows = 0;

        foreach (var mess in messages)
        {
            int length = 0;
            if (mess.SenderType == SenderType.CurrentUser)
            {
                length = mess.Content.Length;
            }
            else
            {
                length = mess.Content.Length + mess.SenderName.Length + 2;
            }

            checked
            {
                while (length > width)
                {
                    textMarginOverflows++;
                    length -= width;
                }
            }
        }

        int totalRows = textMarginOverflows;
        ChatMessage? lastMessage = null;
        foreach (var m in messages)
        {
            totalRows++;
            if (lastMessage == null || m.SenderName != lastMessage.SenderName)
            {
                //Add extra empty space between user messages same sender
                totalRows++;
            }
            lastMessage = m;
        }

        int rowsToTrim = totalRows - height - 1;
        if (rowsToTrim > 0)
        {
            for (int i = 0; i < rowsToTrim; i += 2)
            {
                //TODO: bug
                messages.RemoveAt(0);
            }
        }

        int y = 0;
        lastMessage = null;
        foreach (var m in messages)
        {
            //Only add linespacing if different messages
            if (lastMessage != null && lastMessage.SenderName == m.SenderName)
            {
                y--;
            }

            //<sender>: <message> if not current user
            string? frameMess = null;
            if (m.SenderType == SenderType.CurrentUser)
            {
                frameMess = m.Content;
            }
            else
            {
                frameMess = $"{m.SenderName}: {m.Content}";
            }

            if (m.SenderType != SenderType.CurrentUser) //align chat to the left
            {
                for (int x = 0; x < frameMess.Length; x++)
                {
                    frame[x, y] = frameMess[x];
                }
            }
            else
            {
                //          align chat to the right
                //TODO: add row for x overflow
                int left = width - frameMess.Length;
                int charPtr = frameMess.Length - 1;
                for (int x = width - 1; x >= left; x--)
                {
                    frame[x, y] = frameMess[charPtr];
                    charPtr--;
                }
            }

            y += 2;
            lastMessage = m;
        }
    }

    private void SyncChatMessages(List<ChatMessage> messages)
    {
        int totalRows = 0;
        ChatMessage? lastMessage = null;
        foreach (var mess in messages)
        {
            totalRows++;
            if (lastMessage == null || mess.SenderName != lastMessage.SenderName)
            {
                //Add extra empty space between user messages
                totalRows++;
            }
            lastMessage = mess;
        }

        int chatWindowHeight = Console.WindowHeight - ShellSettings.PromptHeight;
        bool chatOverflow = chatWindowHeight < totalRows;

        if (chatOverflow)
        {

            //Clear the chat window
            for (int y = 0; y < chatWindowHeight; y++)
            {
                for (int x = 0; x < ShellSettings.FrameBufferWidth; x++)
                {
                    _frameBuffer[x, y] = '\0';
                }
            }

            //messages.Reverse();
            //lastMessage = null;
            //int y = chatWindowHeight - 1;

            //foreach (var message in messages)
            //{
            //    for (int x = 0; x < message.Content.Length; x++)
            //    {

            //    }

            //    if (lastMessage != null && lastMessage.SenderName == message.SenderName)
            //    {

            //    }

            //}

            return;
        }


        bool previousMessageIsCurrentUser = false;
        int previousY = -1;


        for (int i = 0; i < messages.Count; i++)
        {
            var message = messages[i];

            var chatMessage = message.SenderType == SenderType.CurrentUser ? new String('\0', ShellSettings.CurrentUserMessageIndentation) + message.Content : $"{message.SenderName}: {message.Content}";

            int y = -1;
            //To enable current user messages to appear underneath each other
            if (previousY > -1 && previousMessageIsCurrentUser && message.SenderType == SenderType.CurrentUser)
            {
                y = previousY + 1;
            }
            //ChatMessage spacing on incoming messages
            else
            {
                y = i * (ShellSettings.MessageSpacing + 1);
            }

            for (int x = 0; x < chatMessage.Length; x++)
            {
                var c = chatMessage[x];
                _frameBuffer[x, y] = c;
            }

            previousMessageIsCurrentUser = message.SenderType == SenderType.CurrentUser;
            previousY = y;
        }
    }

    private void Render(int windowWidth, int windowHeight)
    {
        for (int x = 0; x < windowWidth; x++)
        {
            for (int y = 0; y < windowHeight; y++)
            {
                if (_frameBuffer[x, y] != _currentBuffer[x, y])
                {
                    try
                    {
                        Console.SetCursorPosition(x, y);

                        if (_frameBuffer[x, y] == '\0')
                        {
                            //Console.Write(' ');
                            Console.CursorLeft--;
                            continue;
                        }
                        else
                        {
                            Console.Write(_frameBuffer[x, y]);
                        }
                    }
                    catch (IOException ex)
                    {
                        if (!ex.Message.Contains("The parameter is incorrect"))
                        {
                            throw;
                        }
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        //Allow the console window to be 0
                    }
                    catch (Exception ex)
                    {
                        var currentChar = _currentBuffer[x, y];
                        var frameChar = _frameBuffer[x, y];
                        throw new ShellException($"Exception when trying to draw to console... Console.WindowWidth {Console.WindowWidth}, y = '{x}', Console.WindowHeight: '{Console.WindowHeight}', y: '{y}', currentChar: '{(currentChar == '\0' ? "null" : currentChar)}', frameChar: '{(frameChar == '\0' ? "null" : frameChar)}'", ex);
                    }
                }
                _currentBuffer[x, y] = _frameBuffer[x, y];
            }

        }
    }
}


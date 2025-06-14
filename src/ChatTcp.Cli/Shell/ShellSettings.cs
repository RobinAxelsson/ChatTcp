﻿

using System.Dynamic;

namespace ChatTcp.Cli.Shell;

internal static class ShellSettings
{
    public const string Prompt = "chat>";
    public const int PromptHeight = 3;
    public const int MessageSpacing = 1;
    public const int FrameBufferWidth = 1000;
    public const int FrameBufferHeight = 500;
    public const int RefreshRate = 50; //milliseconds
    public const int KeyHandlerDelay = 10;
    public const int CurrentUserMessageIndentation = 20;
    public const char Cursor = '█';
}


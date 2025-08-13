// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace ChatTcp.Cli
{
    internal interface IConsoleAdapter
    {
        void Clear();
        void SetCursorPosition(int x, int y);
    }
}
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace ChatTcp.Cli.Shell
{
    [Serializable]
    internal class ShellException : Exception
    {
        public ShellException()
        {
        }

        public ShellException(string? message) : base(message)
        {
        }

        public ShellException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}

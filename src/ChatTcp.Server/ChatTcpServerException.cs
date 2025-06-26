// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace ChatTcp.Server
{
    [Serializable]
    internal class ChatTcpServerException : Exception
    {
        public ChatTcpServerException()
        {
        }

        public ChatTcpServerException(string? message) : base(message)
        {
        }

        public ChatTcpServerException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}

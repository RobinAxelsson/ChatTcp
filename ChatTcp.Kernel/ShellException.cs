// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace ChatTcp.Kernel
{
    [Serializable]
    internal class ChatTcpKernelException : Exception
    {
        public ChatTcpKernelException()
        {
        }

        public ChatTcpKernelException(string? message) : base(message)
        {
        }

        public ChatTcpKernelException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}

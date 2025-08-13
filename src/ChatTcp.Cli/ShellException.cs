// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace ChatTcp.Cli
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

    [Serializable]
    internal class InvalidStateException : Exception
    {
        public InvalidStateException()
        {
        }

        public InvalidStateException(object? obj) : base(obj == null ? "null" : JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true }))
        {
        }

        public InvalidStateException(object? obj, Exception? innerException) : base(obj == null ? "null" : JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true }), innerException)
        {
        }
    }
}

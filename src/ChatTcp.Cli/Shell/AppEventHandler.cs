// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ChatTcp.Cli.ConsoleUi;

namespace ChatTcp.Cli;

internal class AppEventHandler
{
    private AppState _appState;

    public AppEventHandler(AppState appState) => _appState = appState;

    public void Handle(AppEvent? appEvent)
    {
        if(appEvent == null)
        {
            throw new ArgumentNullException(nameof(appEvent));
        }

        switch (appEvent)
        {
            case TextInputEvent textEvent:
                _appState.InputBuffer += textEvent.Character;
                break;

            case SendMessageEvent:
                if(_appState.InputBuffer.Length > 0)
                {
                    _appState.Messages.Add(new Message("", _appState.InputBuffer, true));
                    _appState.InputBuffer = "";
                }
                break;

            case BackspaceEvent:
                if (_appState.InputBuffer.Length > 0)
                    _appState.InputBuffer = _appState.InputBuffer[..^1];
                break;

            case QuitEvent:
                Console.WriteLine("Exiting...");
                Environment.Exit(0);
                break;

            default:
                Console.WriteLine("Unknown event received.");
                break;
        }
    }
}

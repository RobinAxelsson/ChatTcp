// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ChatTcp.Cli.ConsoleUi;

namespace ChatTcp.Cli;

internal class AppEventHandler
{
    private readonly AppState _appState;
    private readonly CancellationTokenSource _cts;

    public AppEventHandler(AppState appState, CancellationTokenSource cts)
    {
        _appState = appState;
        _cts = cts;
    }


    public void Handle(AppEvent? appEvent)
    {
        if(appEvent == null)
        {
            throw new ArgumentNullException(nameof(appEvent));
        }

        switch (appEvent)
        {
            case CharInputEvent textEvent:
                _appState.InputBuffer += textEvent.Character;
                _appState.CursorIndex++;
                break;

            case SendMessageEvent:
                if(_appState.InputBuffer.Length > 0)
                {
                    _appState.Messages.Add(new Message("", _appState.InputBuffer, true));
                    _appState.InputBuffer = "";
                    _appState.CursorIndex = 0;
                }
                break;

            case BackspaceEvent:
                if (_appState.InputBuffer.Length > 0)
                {
                    _appState.InputBuffer = _appState.InputBuffer[..^1];
                    _appState.CursorIndex--;
                }
                break;

            case QuitEvent:
                _cts.Cancel();
                break;

            default:
                Console.WriteLine("Unknown event received.");
                break;
        }
    }
}

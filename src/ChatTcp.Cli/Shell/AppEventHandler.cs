// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reactive.Linq;
using System.Reactive.Subjects;
using ChatTcp.Cli.Shell.Models;

namespace ChatTcp.Cli.Shell;

internal class AppEventHandler
{
    private readonly Subject<AppState> _appStateStream = new();
    private readonly AppState _appState;
    private readonly CancellationTokenSource _cts;
    public IObservable<AppState> Events => _appStateStream.AsObservable();

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
                _appStateStream.OnNext(_appState);
                break;

            case SendMessageEvent:
                if(_appState.InputBuffer.Length > 0)
                {
                    _appState.Messages.Add(ChatMessage.FromCurrentUser(_appState.InputBuffer));
                    _appState.InputBuffer = "";
                    _appState.CursorIndex = 0;
                    _appStateStream.OnNext(_appState);
                }
                break;

            case BackspaceEvent:
                if (_appState.InputBuffer.Length > 0)
                {
                    _appState.InputBuffer = _appState.InputBuffer[..^1];
                    _appState.CursorIndex--;
                }
                _appStateStream.OnNext(_appState);
                break;

            case MessageReceivedEvent receiveMessageEvent:
                _appState.Messages.Add(receiveMessageEvent.ChatMessage);
                _appStateStream.OnNext(_appState);
                break;

            case QuitEvent:
                _cts.Cancel();
                break;

            default:
                throw new ArgumentException("Unknown app event");
        }
    }
}

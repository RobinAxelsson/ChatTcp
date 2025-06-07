// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks.Dataflow;
using ChatTcp.Cli.Shell.Models;

namespace ChatTcp.Cli.Shell;

internal class AppEventHandler
{
    private AppState _lastAppState = AppState.Debug;
    private readonly Subject<AppState> _appStateStream = new();
    private readonly Subject<ChatMessage> _chatMessageStream = new();
    private readonly CancellationTokenSource _cts;
    private List<ChatMessage> _chatMessages = new();
    public IObservable<AppState> AppStateStream => _appStateStream.AsObservable();
    public IObservable<ChatMessage> ChatMessageStream => _chatMessageStream.AsObservable();

    public AppEventHandler(CancellationTokenSource cts)
    {
        _cts = cts;
    }

    public void Handle(AppEvent? appEvent)
    {
        if(appEvent == null)
        {
            throw new ArgumentNullException(nameof(appEvent));
        }

        AppState? appState = null;

        switch (appEvent)
        {
            case ConsoleStartupEvent consoleStartupEvent:
                appState = _lastAppState with { WindowHeight = consoleStartupEvent.WindowHeight, WindowWidth = consoleStartupEvent.WindowWidth };
                break;

            case WindowResizedEvent windowResized:
                appState = _lastAppState with { WindowHeight = windowResized.Height, WindowWidth = windowResized.Width };
                break;

            case CharInputEvent charInputEvent:
                appState = _lastAppState with {
                    InputBuffer = _lastAppState.InputBuffer + charInputEvent.Chr,
                    CursorIndex = _lastAppState.CursorIndex + 1
                };
                break;

            case PressEnterEvent:
                if(_lastAppState.InputBuffer.Length > 0)
                {
                    var newChatMessage = ChatMessage.FromCurrentUser(_lastAppState.InputBuffer);
                    _chatMessages.Add(newChatMessage);

                    appState = _lastAppState with
                    {
                        Messages = _chatMessages,
                        InputBuffer = "",
                        CursorIndex = -1
                    };

                    _chatMessageStream.OnNext(newChatMessage);

                }
                break;

            case BackspaceEvent:
                if (_lastAppState.InputBuffer.Length > 0)
                {
                    appState = _lastAppState with { InputBuffer = _lastAppState.InputBuffer[..^1], CursorIndex = _lastAppState.CursorIndex - 1 };
                }
                break;

            case NetworkReceiveEvent receiveMessageEvent:
                _chatMessages.Add(receiveMessageEvent.ChatMessage);
                appState = _lastAppState with { Messages = _chatMessages };
                break;

            case PressEscapeEvent:
                _cts.Cancel();
                break;

            default:
                throw new ArgumentException("Unknown app event");
        }
        if(appState != null)
        {
            _appStateStream.OnNext(appState);
            _lastAppState = appState;
        }
    }
}

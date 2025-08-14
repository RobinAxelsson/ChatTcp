//Instructions chatGpt:
//You are a .NET senior developer
//You value simple design solutions over complex
//You use braces after if statements if they are oneliners
//Use => for oneline Get Properties else braces
//skip braces for throw new Exception if possible
//Few comments if not other specified
//You have memory allocation in mind but dont sacrifice simplicity
//You like to keep entities easy to access and easy to iterate
//You value entity data integrity
//You throw InvalidStateException on any bad state
//When you are ready question if anything is wrong, or not compiling
//dont reaply previous code if not instructed
//Always fix TODOs and remove the TODO comment if implemented
//Never remove these instruction lines


using System.Collections.Concurrent;

namespace ChatTcp.Cli;

internal static class IdHelper
{
    internal static string Create()
    {
        return Guid.CreateVersion7().ToString("n");
    }
}

internal abstract record EntityBase
{
    internal string Id { get; init; } = default!;
}

internal sealed record ChatMessageEntity : EntityBase
{
    internal string UserId { get; init; } = default!;
    internal string ChatId { get; init; } = default!;
    internal string Message { get; init; } = default!;

    internal ChatMessageEntity(string userId, string chatId, string message)
    {
        Id = IdHelper.Create();
        UserId = userId;
        ChatId = chatId;
        Message = message;
    }
}

internal sealed record ChatEntity : EntityBase
{
    internal ChatEntity()
    {
        Id = IdHelper.Create();
    }
}

internal sealed record UserEntity : EntityBase
{
    internal string Username { get; init; } = string.Empty;
    internal HashSet<string> JoinedChatIds { get; init; }

    internal UserEntity(string username, IEnumerable<string>? joinedChatIds = null)
    {
        Id = IdHelper.Create();
        Username = username;
        JoinedChatIds = joinedChatIds is null
            ? new HashSet<string>(StringComparer.Ordinal)
            : new HashSet<string>(joinedChatIds, StringComparer.Ordinal);
    }
}

internal interface IChatMessageActor
{
    void OnChatMessageCreated(ChatMessageEntity message);
    void CreateChatMessage(MessageDto message);
}

internal record MessageDto(string UserId, string ChatId, string Message);

internal class OrderedConcurrentDictionary<T> where T : EntityBase
{
    private readonly ConcurrentDictionary<string, T> _byId = new();

    internal int Count => _byId.Count;

    internal IReadOnlyList<T> ValuesOrdered => _byId.Values.OrderBy(v => v.Id, StringComparer.Ordinal).ToList();

    internal bool ContainsId(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) { return false; }
        return _byId.ContainsKey(id);
    }

    internal bool TryGet(string id, out T? value)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            value = default;
            return false;
        }

        return _byId.TryGetValue(id, out value!);
    }

    internal void Add(T item)
    {
        if (item is null) throw new InvalidStateException("Cannot add null item.");
        if (string.IsNullOrWhiteSpace(item.Id)) throw new InvalidStateException("Item must have an Id.");
        if (!_byId.TryAdd(item.Id, item)) throw new InvalidStateException($"Duplicate Id '{item.Id}'.");
    }

    internal void AddRange(IEnumerable<T> items)
    {
        if (items is null) throw new InvalidStateException("Items collection is null.");

        foreach (var item in items)
        {
            Add(item);
        }
    }

    internal bool Remove(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new InvalidStateException("Id is required.");
        return _byId.TryRemove(id, out _);
    }

    internal bool Update(T item)
    {
        if (item is null) throw new InvalidStateException("Cannot update with null item.");
        if (string.IsNullOrWhiteSpace(item.Id)) throw new InvalidStateException("Item must have an Id.");
        return _byId.AddOrUpdate(item.Id, _ => item, (_, __) => item) is not null;
    }

    internal void Clear()
    {
        _byId.Clear();
    }
}

internal class EntityStore
{
    private readonly ConcurrentBag<IChatMessageActor> _chatMessageActors;
    private readonly OrderedConcurrentDictionary<UserEntity> _users;
    private readonly OrderedConcurrentDictionary<ChatEntity> _chats;
    private readonly OrderedConcurrentDictionary<ChatMessageEntity> _chatMessages;

    internal IReadOnlyList<UserEntity> Users => _users.ValuesOrdered;

    internal IReadOnlyList<ChatEntity> Chats => _chats.ValuesOrdered;

    internal IReadOnlyList<ChatMessageEntity> ChatMessages => _chatMessages.ValuesOrdered;

    internal EntityStore()
    {
        _users = new OrderedConcurrentDictionary<UserEntity>();
        _chats = new OrderedConcurrentDictionary<ChatEntity>();
        _chatMessages = new OrderedConcurrentDictionary<ChatMessageEntity>();
        _chatMessageActors = new ConcurrentBag<IChatMessageActor>();
    }

    internal void RegisterActor(IChatMessageActor actor)
    {
        if (actor is null) throw new InvalidStateException("Actor cannot be null.");
        _chatMessageActors.Add(actor);
    }

    internal ChatMessageEntity CreateMessage(MessageDto dto)
    {
        if (dto is null) throw new InvalidStateException("Message dto is null.");
        if (string.IsNullOrWhiteSpace(dto.UserId)) throw new InvalidStateException("UserId is required.");
        if (string.IsNullOrWhiteSpace(dto.ChatId)) throw new InvalidStateException("ChatId is required.");
        if (string.IsNullOrWhiteSpace(dto.Message)) throw new InvalidStateException("Message is required.");
        if (!_users.ContainsId(dto.UserId)) throw new InvalidStateException($"User '{dto.UserId}' does not exist.");
        if (!_chats.ContainsId(dto.ChatId)) throw new InvalidStateException($"Chat '{dto.ChatId}' does not exist.");

        var entity = new ChatMessageEntity(dto.UserId, dto.ChatId, dto.Message);
        _chatMessages.Add(entity);

        foreach (var actor in _chatMessageActors)
        {
            actor.OnChatMessageCreated(entity);
        }

        return entity;
    }

    internal IReadOnlyList<ChatMessageEntity> CreateMessages(List<MessageDto> messages)
    {
        if (messages is null) throw new InvalidStateException("Messages list is null.");
        if (messages.Count == 0) return Array.Empty<ChatMessageEntity>();

        foreach (var m in messages)
        {
            if (m is null) throw new InvalidStateException("Message dto is null.");
            if (string.IsNullOrWhiteSpace(m.UserId)) throw new InvalidStateException("UserId is required.");
            if (string.IsNullOrWhiteSpace(m.ChatId)) throw new InvalidStateException("ChatId is required.");
            if (string.IsNullOrWhiteSpace(m.Message)) throw new InvalidStateException("Message is required.");
            if (!_users.ContainsId(m.UserId)) throw new InvalidStateException($"User '{m.UserId}' does not exist.");
            if (!_chats.ContainsId(m.ChatId)) throw new InvalidStateException($"Chat '{m.ChatId}' does not exist.");
        }

        var created = new List<ChatMessageEntity>(messages.Count);

        foreach (var m in messages)
        {
            var entity = new ChatMessageEntity(m.UserId, m.ChatId, m.Message);
            _chatMessages.Add(entity);
            created.Add(entity);
        }

        foreach (var entity in created)
        {
            foreach (var actor in _chatMessageActors)
            {
                actor.OnChatMessageCreated(entity);
            }
        }

        return created;
    }

    internal ChatEntity CreateChat()
    {
        var chat = new ChatEntity();
        _chats.Add(chat);
        return chat;
    }

    internal IReadOnlyList<ChatEntity> CreateChats(int count)
    {
        if (count <= 0) throw new InvalidStateException("Count must be positive.");

        var list = new List<ChatEntity>(count);

        for (var i = 0; i < count; i++)
        {
            var chat = new ChatEntity();
            _chats.Add(chat);
            list.Add(chat);
        }

        return list;
    }

    internal UserEntity CreateUser(string username, IEnumerable<string>? joinedChatIds = null)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new InvalidStateException("Username is required.");

        var ids = joinedChatIds is null ? Array.Empty<string>() : joinedChatIds;

        foreach (var id in ids)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new InvalidStateException("Joined chat id is invalid.");
            if (!_chats.ContainsId(id)) throw new InvalidStateException($"Joined chat '{id}' does not exist.");
        }

        var user = new UserEntity(username, ids);
        _users.Add(user);
        return user;
    }

    internal IReadOnlyList<UserEntity> CreateUsers(List<(string Username, IEnumerable<string>? JoinedChatIds)> users)
    {
        if (users is null) throw new InvalidStateException("Users list is null.");
        if (users.Count == 0) return Array.Empty<UserEntity>();

        foreach (var (username, joinedChatIds) in users)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new InvalidStateException("User must have a username.");

            if (joinedChatIds is null)
            {
                continue;
            }

            foreach (var id in joinedChatIds)
            {
                if (string.IsNullOrWhiteSpace(id)) throw new InvalidStateException("Joined chat id is invalid.");
                if (!_chats.ContainsId(id)) throw new InvalidStateException($"Joined chat '{id}' does not exist.");
            }
        }

        var created = new List<UserEntity>(users.Count);

        foreach (var (username, joinedChatIds) in users)
        {
            var u = new UserEntity(username, joinedChatIds);
            _users.Add(u);
            created.Add(u);
        }

        return created;
    }

    internal IReadOnlyList<string> GetChatMessageText(ChatEntity chat)
    {
        if (chat is null) throw new InvalidStateException("Chat is null.");
        if (!_chats.ContainsId(chat.Id)) throw new InvalidStateException($"Chat '{chat.Id}' does not exist.");

        var texts = _chatMessages
            .ValuesOrdered
            .Where(m => m.ChatId == chat.Id)
            .Select(m => m.Message)
            .ToList();

        return texts;
    }

    internal IReadOnlyList<UserEntity> GetChatUsers(ChatEntity chat)
    {
        if (chat is null) throw new InvalidStateException("Chat is null.");
        if (!_chats.ContainsId(chat.Id)) throw new InvalidStateException($"Chat '{chat.Id}' does not exist.");

        var users = _users
            .ValuesOrdered
            .Where(u => u.JoinedChatIds.Contains(chat.Id))
            .ToList();

        return users;
    }
}

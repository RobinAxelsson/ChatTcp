using System.Collections.Concurrent;

namespace ChatTcp.Kernel;

//Instructions chatGpt:
//You are a .NET senior developer
//You value simple design solutions over complex
//You almost always use braces over oneliners
//Few comments if not other specified
//You have memory allocation in mind but dont sacrifice simplicity
//You like to keep entities easy to access and easy to iterate
//You value entity data integrity
//You throw on any bad state
//When you are ready question if anything is wrong, or not compiling
//dont reaply previous code if not instructed
//Alway fix TODOs

//TODO replace oneliners with braces
//TODO move business rules from entities to Create method
internal static class IdHelper
{
    internal static string Create() => Guid.CreateVersion7().ToString("n");
}

internal sealed class InvalidStateException : Exception
{
    internal InvalidStateException(string message) : base(message) { }
    internal InvalidStateException(string message, Exception inner) : base(message, inner) { }
}

internal abstract record HasId
{
    internal string Id { get; init; } = default!;
}

//TODO seal all the entities
internal record ChatMessageEntity : HasId
{
    internal string UserId { get; }
    internal string ChatId { get; }
    internal string Message { get; }

    internal ChatMessageEntity(string userId, string chatId, string message)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new InvalidStateException("UserId is required.");
        if (string.IsNullOrWhiteSpace(chatId)) throw new InvalidStateException("ChatId is required.");
        if (string.IsNullOrWhiteSpace(message)) throw new InvalidStateException("Message is required.");
        Id = IdHelper.Create();
        UserId = userId;
        ChatId = chatId;
        Message = message;
    }
}

internal record ChatEntity : HasId
{
    internal ChatEntity()
    {
        Id = IdHelper.Create();
    }
}

internal record UserEntity : HasId
{
    internal string Username { get; }
    //TODO implement hashset
    internal List<ChatEntity> JoinedChats { get; }

    internal UserEntity(string username, IEnumerable<ChatEntity>? joinedChats = null)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new InvalidStateException("Username is required.");
        Id = IdHelper.Create();
        Username = username;
        JoinedChats = joinedChats?.ToList() ?? new List<ChatEntity>();
    }
}

internal interface IChatMessageActor
{
    void OnChatMessageCreated(ChatMessageEntity message);
    void CreateChatMessage(MessageDto message);
}

internal record MessageDto(string UserId, string ChatId, string Message);

internal class OrderedConcurrentDictionary<T> where T : HasId
{
    private readonly ConcurrentDictionary<string, T> _byId = new();

    internal int Count => _byId.Count;

    internal IReadOnlyList<T> ValuesOrdered =>
        _byId.Values.OrderBy(v => v.Id, StringComparer.Ordinal).ToList();

    internal bool ContainsId(string id) =>
        !string.IsNullOrWhiteSpace(id) && _byId.ContainsKey(id);

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
        if (!_byId.TryAdd(item.Id, item))
            throw new InvalidStateException($"Duplicate Id '{item.Id}'.");
    }

    internal void AddRange(IEnumerable<T> items)
    {
        if (items is null) throw new InvalidStateException("Items collection is null.");
        foreach (var item in items) Add(item);
    }

    internal void Clear() => _byId.Clear();
}

internal class EntityStore
{
    private readonly List<IChatMessageActor> _chatMessageActors;

    //TODO I want this datatype accessed internally but readonly, private mutable with update or delete
    internal OrderedConcurrentDictionary<UserEntity> Users { get; }
    internal OrderedConcurrentDictionary<ChatEntity> Chats { get; }
    internal OrderedConcurrentDictionary<ChatMessageEntity> ChatMessages { get; }

    internal EntityStore()
    {
        Users = new OrderedConcurrentDictionary<UserEntity>();
        Chats = new OrderedConcurrentDictionary<ChatEntity>();
        ChatMessages = new OrderedConcurrentDictionary<ChatMessageEntity>();
        _chatMessageActors = new List<IChatMessageActor>();
    }

    internal void RegisterActor(IChatMessageActor actor)
    {
        if (actor is null) throw new InvalidStateException("Actor cannot be null.");
        _chatMessageActors.Add(actor);
    }

    //TODO implement create single message as well
    internal void CreateMessages(List<ChatMessageEntity> messages)
    {
        if (messages is null) throw new InvalidStateException("Messages list is null.");
        if (messages.Count == 0) return;
        foreach (var m in messages)
        {
            if (m is null) throw new InvalidStateException("Message is null.");
            if (!Users.ContainsId(m.UserId)) throw new InvalidStateException($"User '{m.UserId}' does not exist.");
            if (!Chats.ContainsId(m.ChatId)) throw new InvalidStateException($"Chat '{m.ChatId}' does not exist.");
        }

        //TODO notify after the loop
        foreach (var m in messages)
        {
            ChatMessages.Add(m);
            foreach (var actor in _chatMessageActors)
                actor.OnChatMessageCreated(m);
        }
    }

    //TODO create ChatEntity
    //TODO create ChatEntities

    internal void CreateUsers(List<UserEntity> users)
    {
        if (users is null) throw new InvalidStateException("Users list is null.");
        if (users.Count == 0) return;
        foreach (var u in users)
        {
            if (u is null) throw new InvalidStateException("User is null.");
            if (string.IsNullOrWhiteSpace(u.Username)) throw new InvalidStateException("User must have a username.");
            foreach (var c in u.JoinedChats)
                if (!Chats.ContainsId(c.Id))
                    throw new InvalidStateException($"Joined chat '{c.Id}' does not exist.");
        }
        Users.AddRange(users);
    }
}

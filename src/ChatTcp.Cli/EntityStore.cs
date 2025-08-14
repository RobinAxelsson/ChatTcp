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

internal class EntityStore
{
    public OrderedConcurrentDictionary<UserEntity> Users { get; }
    public OrderedConcurrentDictionary<ChatEntity> Chats { get; }
    public OrderedConcurrentDictionary<ChatMessageEntity> ChatMessages { get; }

    public EntityStore()
    {
        Users = new OrderedConcurrentDictionary<UserEntity>();
        Chats = new OrderedConcurrentDictionary<ChatEntity>();
        ChatMessages = new OrderedConcurrentDictionary<ChatMessageEntity>();
    }
}

internal static class EntityManager
{
    //TODO create entities
}


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

internal interface IChatMessageSubscriber
{
    void OnChatMessageCreated(ChatMessageEntity message);
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

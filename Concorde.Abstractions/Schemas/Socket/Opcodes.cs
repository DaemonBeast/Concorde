using System.Collections.Concurrent;

namespace Concorde.Abstractions.Schemas.Socket;

public static class Opcodes
{
    public static ConcurrentDictionary<int, Type> Schemas { get; }
    
    public static ConcurrentDictionary<int, Type> DataSchemas { get; }

    static Opcodes()
    {
        Schemas = new ConcurrentDictionary<int, Type>();
        
        _ = Schemas.TryAdd((int) Ids.Heartbeat, typeof(HeartbeatMessage));
        _ = Schemas.TryAdd((int) Ids.Identity, typeof(IdentifyMessage));
        _ = Schemas.TryAdd((int) Ids.PresenceUpdate, typeof(PresenceMessage));
        _ = Schemas.TryAdd((int) Ids.Resume, typeof(ResumeMessage));
        _ = Schemas.TryAdd((int) Ids.InvalidSession, typeof(InvalidSessionMessage));
        _ = Schemas.TryAdd((int) Ids.Hello, typeof(HelloMessage));

        DataSchemas = new ConcurrentDictionary<int, Type>();
        
        _ = DataSchemas.TryAdd((int) Ids.Heartbeat, typeof(int?));
        _ = DataSchemas.TryAdd((int) Ids.Identity, typeof(IdentifyData));
        _ = DataSchemas.TryAdd((int) Ids.Resume, typeof(ResumeData));
        _ = DataSchemas.TryAdd((int) Ids.InvalidSession, typeof(bool));
        _ = DataSchemas.TryAdd((int) Ids.Hello, typeof(HelloData));
    }
    
    public enum Ids
    {
        Dispatch = 0,
        Heartbeat = 1,
        Identity = 2,
        PresenceUpdate = 3,
        VoiceStateUpdate = 4,
        Resume = 6,
        Reconnect = 7,
        RequestGuildMembers = 8,
        InvalidSession = 9,
        Hello = 10,
        HeartbeatAck = 11
    }
}
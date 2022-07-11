﻿using System.Collections.Concurrent;

namespace Concorde.Abstractions.Schemas.Events;

public static class Events
{
    public static ConcurrentDictionary<string, Type> Schemas { get; }

    static Events()
    {
        Schemas = new ConcurrentDictionary<string, Type>();

        _ = Schemas.TryAdd(Names.Ready, typeof(ReadyEvent));
        _ = Schemas.TryAdd(Names.Resumed, typeof(ResumeEvent));
    }
    
    public static class Names
    {
        public const string Ready = "READY";
        public const string Resumed = "RESUMED";
    }
}
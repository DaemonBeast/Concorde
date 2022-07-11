using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Concorde.Abstractions.Schemas.Events;
using Concorde.Abstractions.Schemas.Objects;
using Concorde.Abstractions.Schemas.Socket;

namespace Concorde.Json;

// handle null?

public class DiscordSocketMessageConverter : JsonConverter<IDiscordSocketMessage>
{
    public override IDiscordSocketMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        int? opcode = null;
        int? sequenceNumber = null;
        string? eventName = null;

        var dataReader = reader;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (opcode == null)
                {
                    throw new JsonException();
                }

                return this.CreateMessageFrom(opcode.Value, ref dataReader, options, sequenceNumber, eventName);
            }
            
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case "op":
                {
                    opcode = reader.GetInt32();
                    break;
                }
                case "s":
                {
                    if (reader.TokenType != JsonTokenType.Null)
                    {
                        sequenceNumber = reader.GetInt32();
                    }

                    break;
                }
                case "t":
                {
                    if (reader.TokenType != JsonTokenType.Null)
                    {
                        eventName = reader.GetString();
                    }
                    
                    break;
                }
                case "d":
                {
                    dataReader = reader;
                    reader.Skip();
                    
                    break;
                }
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, IDiscordSocketMessage value, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    private IDiscordSocketMessage CreateMessageFrom(
        int opcode,
        ref Utf8JsonReader dataReader,
        JsonSerializerOptions options,
        int? sequenceNumber,
        string? eventName)
    {
        if (opcode == (int) Opcodes.Ids.Dispatch)
        {
            if (eventName == null || sequenceNumber == null)
            {
                throw new JsonException();
            }

            if (Events.Schemas.TryGetValue(eventName, out var eventType))
            {
                var valueConverter = options.GetConverter(eventType) as JsonConverter<IDiscordEvent>;

                var data = valueConverter?.Read(ref dataReader, eventType, options) ??
                           JsonSerializer.Deserialize(ref dataReader, eventType, options)!;

                var socketMessage = (BaseDispatchDiscordSocketMessage) Activator.CreateInstance(
                    typeof(DispatchDiscordSocketMessage<>).MakeGenericType(eventType), data)!;
                
                socketMessage.Opcode = opcode;
                socketMessage.SequenceNumber = sequenceNumber.Value;
                socketMessage.EventName = eventName;

                return socketMessage;
            }
            else
            {
                var socketMessage = new BaseDispatchDiscordSocketMessage()
                {
                    Opcode = opcode,
                    SequenceNumber = sequenceNumber.Value,
                    EventName = eventName
                };

                return socketMessage;
            }
        }
        
        if (Opcodes.Schemas.TryGetValue(opcode, out var messageType) &&
            Opcodes.DataSchemas.TryGetValue(opcode, out var dataType))
        {
            var valueConverter = options.GetConverter(dataType) as JsonConverter<object>;

            var data = valueConverter?.Read(ref dataReader, dataType, options) ??
                       JsonSerializer.Deserialize(ref dataReader, dataType, options)!;

            var socketMessage =
                (BaseDiscordSocketMessage) Activator.CreateInstance(messageType, data)!;
            
            socketMessage.Opcode = opcode;

            return socketMessage;
        }
        else
        {
            var socketMessage = new BaseDiscordSocketMessage()
            {
                Opcode = opcode
            };

            return socketMessage;
        }
    }
}

/*public class DiscordSocketMessageConverter : JsonConverter<IDiscordSocketMessage>
{
    public override IDiscordSocketMessage Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        int? opcode = null;
        int? sequenceNumber = null;
        string? eventName = null;

        var readerClone = reader;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (opcode == null)
                {
                    throw new JsonException();
                }

                if (opcode == (int) Opcodes.Ids.Dispatch)
                {
                    if (eventName == null ||
                        sequenceNumber == null)
                    {
                        throw new JsonException();
                    }

                    if (Events.Schemas.TryGetValue(eventName, out var eventType))
                    {
                        var valueConverter = options.GetConverter(eventType) as JsonConverter<IDiscordEvent>;

                        var data = (IDiscordEvent) (valueConverter?.Read(ref readerClone, eventType, options) ??
                                                    JsonSerializer.Deserialize(ref readerClone, eventType, options)!);

                        /*var socketMessage =
                            (DispatchDiscordSocketMessage) Activator.CreateInstance(
                                typeof(DispatchDiscordSocketMessage<>).MakeGenericType(eventType))!;*/

                        /*var socketMessage = new DispatchDiscordSocketMessage()
                        {
                            Opcode = opcode.Value,
                            Data = data,
                            SequenceNumber = sequenceNumber.Value,
                            EventName = eventName
                        };

                        /*socketMessage.Opcode = opcode.Value;
                        socketMessage.Data = data;
                        socketMessage.SequenceNumber = sequenceNumber.Value;
                        socketMessage.EventName = eventName;*/

                        /*return socketMessage;
                    }
                    else
                    {
                        var socketMessage = new BaseDispatchDiscordSocketMessage()
                        {
                            Opcode = opcode.Value,
                            SequenceNumber = sequenceNumber.Value,
                            EventName = eventName
                        };

                        return socketMessage;
                    }
                }

                if (Opcodes.DataSchemas.TryGetValue(opcode.Value, out var dataType))
                {
                    var valueConverter = options.GetConverter(dataType) as JsonConverter<object>;

                    var data = valueConverter?.Read(ref readerClone, dataType, options) ??
                               JsonSerializer.Deserialize(ref readerClone, dataType, options)!;

                    /*var socketMessage =
                        (BaseDataDiscordSocketMessage) Activator.CreateInstance(
                            typeof(BaseDataDiscordSocketMessage<>).MakeGenericType(dataType))!;*/

                    /*var socketMessage = new BaseDataDiscordSocketMessage()
                    {
                        Opcode = opcode.Value,
                        Data = data
                    };
                    
                    /*socketMessage.Opcode = opcode.Value;
                    socketMessage.Data = data;*/

                    /*return socketMessage;
                }
                else
                {
                    var socketMessage = new BaseDiscordSocketMessage()
                    {
                        Opcode = opcode.Value
                    };

                    return socketMessage;
                }
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case "op":
                {
                    opcode = reader.GetInt32();
                    break;
                }
                case "s":
                {
                    if (reader.TokenType != JsonTokenType.Null)
                    {
                        sequenceNumber = reader.GetInt32();
                    }

                    break;
                }
                case "t":
                {
                    if (reader.TokenType != JsonTokenType.Null)
                    {
                        eventName = reader.GetString();
                    }
                    
                    break;
                }
                case "d":
                {
                    readerClone = reader;
                    reader.Skip();
                    
                    break;
                }
            }
        }

        throw new JsonException();

        /*var opcode = this.GetValue<int>(ref reader, options, "op", out var skipped, out var readerClone);

        if (opcode == (int) Opcodes.Ids.Dispatch)
        {
            if (skipped)
            {
                var sequenceNumber = 
            }
        }*/

        /*var propertyName = reader.GetString();
        if (propertyName == "op")
        {
            reader.Read();
            var opcode = reader.GetInt32();

            if (opcode == (int) Opcodes.Ids.Dispatch)
            {
                reader.Read();
            }
            
            if (!Opcodes.Schemas.TryGetValue(opcode, out var messageType))
            {
                throw new JsonException();
            }
            
            
        }*/
    /*}

    public override void Write(Utf8JsonWriter writer, IDiscordSocketMessage value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

/*private bool TryGetValue<TValue>(
    ref Utf8JsonReader reader,
    JsonSerializerOptions options,
    string propertyName,
    out TValue value)
    where TValue : struct
{
    var valueType = typeof(TValue);
    var valueConverter = options.GetConverter(valueType) as JsonConverter<TValue>;

    var propName = reader.GetString();
    if (propName == propertyName)
    {
        reader.Read();

        var opcode = valueConverter?.Read(ref reader, valueType, options) ??
                     JsonSerializer.Deserialize<TValue>(ref reader, options);

        skipped = false;
        return opcode;
    }
}

private TValue GetValue<TValue>(
    ref Utf8JsonReader reader,
    JsonSerializerOptions options,
    string propertyName,
    out bool skipped,
    out Utf8JsonReader readerClone)
    where TValue : struct
{
    readerClone = reader;
    
    var valueType = typeof(TValue);
    var valueConverter = options.GetConverter(valueType) as JsonConverter<TValue>;
    
    if (reader.TokenType != JsonTokenType.PropertyName)
    {
        reader.Skip();
        reader.Read();
    }
    
    var propName = reader.GetString();
    if (propName == propertyName)
    {
        reader.Read();

        var opcode = valueConverter?.Read(ref reader, valueType, options) ??
                     JsonSerializer.Deserialize<TValue>(ref reader, options);

        skipped = false;
        return opcode;
    }

    readerClone.Skip();
    readerClone.Skip();

    while (readerClone.Read())
    {
        if (readerClone.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException();
        }

        propName = readerClone.GetString();
        if (propName != propertyName)
        {
            readerClone.Skip();
            readerClone.Skip();
            
            continue;
        }
        
        readerClone.Read();
        var opcode = valueConverter?.Read(ref reader, valueType, options) ??
                     JsonSerializer.Deserialize<TValue>(ref reader, options);

        skipped = true;
        return opcode;
    }

    throw new JsonException();
}
}

/*public class DiscordSocketMessageConverter : JsonConverterFactory
{
public override bool CanConvert(Type typeToConvert)
{
    return typeToConvert.IsGenericType &&
           typeToConvert.GetGenericTypeDefinition() == typeof(IDiscordSocketMessage<>) &&
           typeToConvert.GetGenericArguments()[0].IsAssignableTo(typeof(IDiscordEvent));
}

public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
{
    var eventType = typeToConvert.GetGenericArguments()[0];

    var converter = (JsonConverter) Activator.CreateInstance(
        typeof(DiscordSocketMessageInnerConverter<>).MakeGenericType(eventType),
        BindingFlags.Instance | BindingFlags.Public,
        binder: null,
        args: new object[] { options },
        culture: null)!;

    return converter;
}

private class DiscordSocketMessageInnerConverter<TEvent> : JsonConverter<IDiscordSocketMessage<TEvent>?>
    where TEvent : IDiscordEvent
{
    private readonly JsonConverter<TEvent> _valueConverter;
    private readonly Type _eventType;

    public DiscordSocketMessageInnerConverter(JsonSerializerOptions options)
    {
        this._valueConverter = (JsonConverter<TEvent>) options.GetConverter(typeof(TEvent));
        this._eventType = typeof(TEvent);
    }

    public override IDiscordSocketMessage<TEvent>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return ;
            }
            
            
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, IDiscordSocketMessage<TEvent>? value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
}*/

/*public class DiscordSocketMessageConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsAssignableTo(typeof(IDiscordSocketMessage));
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        
    }
}*/

/*public class DiscordSocketMessageOpcodeConverter : JsonConverter<int?>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsAssignableTo(typeof(IDiscordSocketMessage));
    }

    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var opcode = 0;
        var found = false;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return opcode;
            }
            
            if (!found && reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "op")
            {
                reader.Read();
                
                opcode = reader.GetInt32();
                found = true;
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}*/
using System.Text.Json;
using System.Text.Json.Serialization;
using Concorde.Abstractions.Schemas.Events;
using Concorde.Abstractions.Schemas.Socket;

namespace Concorde.Json;

// handle null?

public class DiscordSocketMessageConverter : JsonConverter<IDiscordSocketMessage>
{
    public override IDiscordSocketMessage Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
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
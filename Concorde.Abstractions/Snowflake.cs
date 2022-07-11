namespace Concorde.Abstractions;

public readonly struct Snowflake
{
    public const ulong DiscordEpoch = 1420070400000;
    
    public ulong Timestamp => (this._value >> 22) + DiscordEpoch;
    public DateTimeOffset Date => DateTimeOffset.FromUnixTimeMilliseconds((long) this.Timestamp);
    
    public int WorkerId => (int) ((this._value & 0x3E0000) >> 17);
    public int ProcessId => (int) ((this._value & 0x1F000) >> 12);
    public int Increment => (int) (this._value & 0xFFF);
    
    private readonly ulong _value;

    public static Snowflake From(string value)
    {
        return new Snowflake(ulong.Parse(value));
    }

    public Snowflake(ulong value)
    {
        this._value = value;
    }

    public override string ToString() => this._value.ToString();

    public static implicit operator ulong(Snowflake s) => s._value;
    public static explicit operator Snowflake(ulong u) => new(u);
    public static explicit operator Snowflake(string s) => From(s);
}
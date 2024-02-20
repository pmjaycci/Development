using MessagePack;

namespace ClientPacket
{
    [MessagePackObject]
    public class Packet
    {
        [Key(0)]
        public int Opcode { get; set; }
    }
    [MessagePackObject]
    public class Start : Packet
    {
        [Key(1)]
        public string? name { get; set; }
        [Key(2)]
        public string? message { get; set; }
    }
    [MessagePackObject]

    public class Update : Packet
    {
        [Key(1)]
        public int state { get; set; }
        [Key(2)]
        public int fieldId { get; set; }
        [Key(3)]
        public string? message { get; set; }
    }
}
namespace ServerPacket
{
    [MessagePackObject]
    public class Packet
    {
        [Key(0)]
        public int Opcode { get; set; }
    }
    [MessagePackObject]

    public class Start : Packet
    {
        [Key(1)]
        public string? name { get; set; }
        [Key(2)]
        public int state { get; set; }
        [Key(3)]
        public int fieldId { get; set; }
        [Key(4)]
        public string? message { get; set; }
    }
    [MessagePackObject]

    public class Update : Packet
    {
        [Key(1)]
        public string? name { get; set; }
        [Key(2)]
        public int state { get; set; }
        [Key(3)]
        public int fieldId { get; set; }
        [Key(4)]
        public string? message { get; set; }
    }
    [MessagePackObject]

    public class Delete : Packet
    {
        [Key(1)]
        public string? name { get; set; }
    }
}

enum Opcode
{
    Start,
    Update,
    Delete,
}

enum State
{
    Idle,
    Move
}
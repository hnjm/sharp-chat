namespace SharpChat.Packets {
    public interface IServerPacket {
        public const char SEPARATOR = '\t';
        string Pack();
    }
}

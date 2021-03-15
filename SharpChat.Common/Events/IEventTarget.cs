namespace SharpChat.Events {
    public interface IEventTarget : IEventHandler {
        string TargetName { get; }
    }
}

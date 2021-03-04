namespace SharpChat.Events {
    public interface IEventHandler {
        void HandleEvent(IEvent evt);
    }
}

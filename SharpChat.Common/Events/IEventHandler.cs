namespace SharpChat.Events {
    public interface IEventHandler {
        void HandleEvent(object sender, IEvent evt);
    }
}

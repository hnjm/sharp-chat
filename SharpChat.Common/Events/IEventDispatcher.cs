namespace SharpChat.Events {
    public interface IEventDispatcher {
        void DispatchEvent(object sender, IEvent evt);
    }
}

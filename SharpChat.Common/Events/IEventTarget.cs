namespace SharpChat.Events {
    public interface IEventTarget {
        void DispatchEvent(IEvent evt);
    }
}

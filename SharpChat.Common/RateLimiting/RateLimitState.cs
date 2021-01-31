namespace SharpChat.RateLimiting {
    public enum RateLimitState : int {
        None,
        Warning,
        Kick,
        Disconnect,
    }
}

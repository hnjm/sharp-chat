namespace SharpChat.Users.Auth {
    public interface IUserAuthClient {
        IUserAuthResponse AttemptAuth(UserAuthRequest request);
    }
}

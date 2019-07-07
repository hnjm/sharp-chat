namespace SharpChat
{
    public static class Constants
    {
        public const char SEPARATOR = '\t';
        public const char MISC_SEPARATOR = '\f';

        public const string LEAVE_NORMAL = @"leave";
        public const string LEAVE_KICK = @"kick";
        public const string LEAVE_FLOOD = @"flood";
        public const string LEAVE_TIMEOUT = @"timeout";

        public const string CTX_USER = @"0";
        public const string CTX_MSG = @"1";
        public const string CTX_CHANNEL = @"2";

        public const string CLEAR_MSGS = @"0";
        public const string CLEAR_USERS = @"1";
        public const string CLEAR_CHANNELS = @"2";
        public const string CLEAR_MSGNUSERS = @"3";
        public const string CLEAR_ALL = @"4";

        public const string MSG_NORMAL = @"0";
        public const string MSG_ERROR = @"1";
    }
}

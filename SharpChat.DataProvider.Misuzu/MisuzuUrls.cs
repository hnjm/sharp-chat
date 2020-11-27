namespace SharpChat.DataProvider.Misuzu {
    public static class MisuzuUrls {
        public const string BASE_URL =
#if DEBUG
            @"https://misuzu.misaka.nl/_sockchat";
#else
            @"https://flashii.net/_sockchat";
#endif

        public const string AUTH = BASE_URL + @"/verify";
        public const string BANS = BASE_URL + @"/bans";
        public const string BUMP = BASE_URL + @"/bump";
    }
}

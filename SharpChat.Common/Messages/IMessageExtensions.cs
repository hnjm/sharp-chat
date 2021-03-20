namespace SharpChat.Messages {
    public static class IMessageExtensions {
        public static string GetSanitisedText(this IMessage msg)
            => msg.Text
                .Replace(@"<", @"&lt;")
                .Replace(@">", @"&gt;")
                .Replace("\n", @" <br/> ")
                .Replace("\t", @"    ");
    }
}

namespace SharpChat {
    public readonly struct ChatColour {
        public const int INHERIT = 0x40000000;

        public int Raw { get; }

        public ChatColour(bool inherit = true) {
            Raw = inherit ? INHERIT : 0;
        }

        public ChatColour(int colour) {
            Raw = colour;
        }

        public bool Inherit => (Raw & INHERIT) > 0;
        public int Red => (Raw >> 16) & 0xFF;
        public int Green => (Raw >> 8) & 0xFF;
        public int Blue => Raw & 0xFF;

        public override string ToString() {
            if (Inherit)
                return @"inherit";
            return string.Format(@"#{0:X6}", Raw);
        }
    }
}

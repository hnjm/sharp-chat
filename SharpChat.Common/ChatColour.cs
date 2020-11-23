namespace SharpChat {
    public class ChatColour {
        public const int INHERIT = 0x40000000;

        public int Raw { get; set; }

        public ChatColour(bool inherit = true) {
            Inherit = inherit;
        }

        public ChatColour(int colour) {
            Raw = colour;
        }

        public bool Inherit {
            get => (Raw & INHERIT) > 0;
            set {
                if (value)
                    Raw |= INHERIT;
                else
                    Raw &= ~INHERIT;
            }
        }

        public int Red {
            get => (Raw >> 16) & 0xFF;
            set {
                Raw &= ~0xFF0000;
                Raw |= (value & 0xFF) << 16;
            }
        }

        public int Green {
            get => (Raw >> 8) & 0xFF;
            set {
                Raw &= ~0xFF00;
                Raw |= (value & 0xFF) << 8;
            }
        }

        public int Blue {
            get => Raw & 0xFF;
            set {
                Raw &= ~0xFF;
                Raw |= value & 0xFF;
            }
        }

        public override string ToString() {
            if (Inherit)
                return @"inherit";
            return string.Format(@"#{0:X6}", Raw);
        }
    }
}

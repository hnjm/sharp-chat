using System;

namespace SharpChat.Http {
    public abstract class HttpHeader {
        public abstract string Name { get; }
        public abstract object Value { get; }

        public override string ToString() {
            return string.Format(@"{0}: {1}", Name, Value);
        }

        public static string NormaliseName(string name) {
            // TODO: normalise name
            return name;
        }
    }
}

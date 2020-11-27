using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpChat {
    public enum ChatRateLimitState {
        None,
        Warning,
        Kick,
    }

    public class ChatRateLimiter {
        private const int FLOOD_PROTECTION_AMOUNT = 30;
        private const int FLOOD_PROTECTION_THRESHOLD = 10;

        private readonly Queue<DateTimeOffset> TimePoints = new Queue<DateTimeOffset>();

        public ChatRateLimitState State {
            get {
                lock (TimePoints) {
                    if (TimePoints.Count == FLOOD_PROTECTION_AMOUNT) {
                        if ((TimePoints.Last() - TimePoints.First()).TotalSeconds <= FLOOD_PROTECTION_THRESHOLD)
                            return ChatRateLimitState.Kick;

                        if ((TimePoints.Last() - TimePoints.Skip(5).First()).TotalSeconds <= FLOOD_PROTECTION_THRESHOLD)
                            return ChatRateLimitState.Warning;
                    }

                    return ChatRateLimitState.None;
                }
            }
        }

        public void AddTimePoint(DateTimeOffset? dto = null) {
            if (!dto.HasValue)
                dto = DateTimeOffset.UtcNow;

            lock (TimePoints) {
                if (TimePoints.Count >= FLOOD_PROTECTION_AMOUNT)
                    TimePoints.Dequeue();

                TimePoints.Enqueue(dto.Value);
            }
        }
    }
}

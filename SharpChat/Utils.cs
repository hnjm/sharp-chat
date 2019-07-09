using System;

namespace SharpChat
{
    public static class Utils
    {
        public static string UnixNow
            => DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        public static string InitialMessage
            => new[] {
                @"This is Agent 2. If you're reading this it means you're either Agent 3 or someone listening in. If you're the latter I request you cease or you will regret it. I've acquired intel on the whereabouts of Agent 1 and you're going to have to help me out recovering her. Agent 4 and Agent 8 are each busy with another mission so we can't rely on their reinforcement either. A large shipment of crabby cakes has been reported missing with a trail of crumbs leading to Octo Valley, those octoslobs must have abducted her by setting up an elaborate trap. Meet me at Cuttlefish Cabin, and be sure to Stay Fresh.",
                @"BECAUSE NUTRITION SUCKED BY THE HUGE MONSTERS, SOR ALL THE PLANTS HAVE SHRIVING ONE. THIS ATMOSPHERE OF PEACE HAS BEEN DESTROYED THE GREEN LAND IS BECOMING TO WASTELAND. AND PEOPLES LIVES WERE ALSO THREATEN BY MONSTER. TILL ONE DAY, A LITTLE HERO CALLED JONY HAS COME UP, HE MUST DEFEAT ALL THESE MONSTERS BY HIS MAGIC FLOWER.",
                @"the new mac pro supports up to 1.5tb ram, maybe it'll be able to run discord",
                @"you have the audacity to call me a fucking homunculus and you're sitting over here drinking curdled coke milk, this is abominable",
                @"you",
                @"you can't escape a man with no capes",
                @"genocide denocide what nintendenocide",
                @"genesis denesis what nintendenis",
                @"school supplies",
                @"shosple colupis",
               }[RNG.Next(0, 9)];
    }
}

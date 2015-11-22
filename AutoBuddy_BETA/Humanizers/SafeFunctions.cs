using EloBuddy;
using EloBuddy.SDK;
using SharpDX;

namespace AutoBuddy.Humanizers
{
    internal static class SafeFunctions
    {
        private static float lastPing;
        private static float lastChat;
        private static readonly SafeShop SafeShop;

        static SafeFunctions()
        {
            SafeShop = new SafeShop();
            lastChat = 0;
        }

        public static void BuyItem(int itemId)
        {
            SafeShop.Buy(itemId);
        }

        public static void BuyItem(ItemId itemId)
        {
            SafeShop.Buy(itemId);
        }

        public static void BuyIfNotOwned(int itemId)
        {
            SafeShop.BuyIfNotOwned(itemId);
        }

        public static void BuyIfNotOwned(ItemId itemId)
        {
            SafeShop.BuyIfNotOwned(itemId);
        }

        public static void Ping(PingCategory cat, Vector3 pos)
        {
            if (lastPing > Game.Time) return;
            lastPing = Game.Time + 1.8f;
            Core.DelayAction(() => TacticalMap.SendPing(cat, pos), RandGen.r.Next(450, 800));
        }

        public static void Ping(PingCategory cat, GameObject target)
        {
            if (lastPing > Game.Time) return;
            lastPing = Game.Time + 1.8f;
            Core.DelayAction(() => TacticalMap.SendPing(cat, target), RandGen.r.Next(450, 800));
        }

        public static void SayChat(string msg)
        {
            if (lastChat > Game.Time) return;
            lastChat = Game.Time + .8f;
            Core.DelayAction(() => Chat.Say(msg), RandGen.r.Next(150, 400));
        }
    }
}
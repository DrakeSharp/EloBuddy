using EloBuddy;

namespace AutoBuddy.Utilities
{
    internal class HeroInfo
    {
        public AIHeroClient hero;

        public HeroInfo(AIHeroClient h)
        {
            hero = h;
            Game.OnNotify += Game_OnNotify;
        }

        public int kills { get; private set; }

        private void Game_OnNotify(GameNotifyEventArgs args)
        {
            if (args.EventId == GameEventId.OnChampionKill && args.NetworkId == hero.NetworkId)
                kills++;
        }
    }
}
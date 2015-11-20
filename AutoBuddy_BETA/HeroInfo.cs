using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;

namespace Buddy_vs_Bot
{
    
    class HeroInfo
    {
        public AIHeroClient hero;
        public int kills;

        public HeroInfo(AIHeroClient h)
        {
            hero = h;
            Game.OnNotify += Game_OnNotify;
        }

        void Game_OnNotify(GameNotifyEventArgs args)
        {
            if (args.EventId == GameEventId.OnChampionKill && args.NetworkId == hero.NetworkId)
                kills++;
        }
    }
}

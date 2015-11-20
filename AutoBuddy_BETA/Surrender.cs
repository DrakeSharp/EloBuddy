using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;

namespace Buddy_vs_Bot
{
    class Surrender
    {
        public Surrender()
        {
            Game.OnNotify += Game_OnNotify;
        }

        void Game_OnNotify(GameNotifyEventArgs args)
        {
            if (args.EventId == GameEventId.OnKill || args.EventId == GameEventId.OnDelete ||
                args.EventId == GameEventId.OnMinionDenied||args.EventId==GameEventId.OnDie||args.EventId==GameEventId.OnShopMenuOpen||args.EventId==GameEventId.OnNeutralMinionKill) return;
            if(args.EventId==GameEventId.OnSurrenderVote)
                Core.DelayAction(()=>Chat.Say("/ff"), RandGen.r.Next(500, 5000));
        }

    }
}

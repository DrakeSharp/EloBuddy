using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;

namespace Buddy_vs_Bot
{
    class LocalAwareness
    {
        private readonly List<HeroInfo> heroTable;
        private readonly HeroInfo me;
        public LocalAwareness()
        {
            heroTable=new List<HeroInfo>();
            foreach (AIHeroClient h in EntityManager.Heroes.AllHeroes)
            {
                if (h.IsMe)
                {
                    me = new HeroInfo(h);
                    heroTable.Add(me);
                }
                else
                    heroTable.Add(new HeroInfo(h));
            }

            //Drawing.OnDraw += Drawing_OnDraw;
        }


        void Drawing_OnDraw(EventArgs args)
        {
            float y = 230;
            foreach (var VARIABLE in EntityManager.Heroes.AllHeroes)
            {
                Drawing.DrawText(5, y, System.Drawing.Color.Gold, VARIABLE.GoldTotal+" ");
                y += 18;
            }
        }

        public float LocalDomination(Vector3 pos)
        {
            float danger = 0;
            foreach (HeroInfo h in heroTable.Where(h => h.hero.Distance(pos) < 900 && h.hero.IsVisible()))
            {
                if (h.hero.Health <= 0) continue;
                danger += (-0.0042857142857143f * (h.hero.Distance(pos) + 100) + 4.4285714285714f) *HeroStrength(h) * (h.hero.IsAlly ? -1 : 1);
            }
            foreach (Obj_AI_Minion tt in ObjectManager.Get<Obj_AI_Minion>().Where(min=>min.Health>0&&min.Distance(pos)<600&&min.Name.StartsWith("H28-G")))
            {
                danger += 10000*(tt.IsEnemy?1:-1);
            }
            if (AutoWalker.p.GetNearestTurret().Distance(pos) < 1000) danger += 35000;
            if (AutoWalker.p.GetNearestTurret(false).Distance(pos) < 400) danger -= 35000;
            return danger;
        }

        public float HeroStrength(HeroInfo h)
        {
            return (h.hero.HealthPercent)*(100 + h.hero.Level*10 + h.kills*5);
        }

        public float MyStrength()
        {
            return HeroStrength(me);
        }
        public float HeroStrength(AIHeroClient h)
        {
            return HeroStrength(heroTable.First(he => he.hero == h));
        }

        public float LocalDomination(Obj_AI_Base ob)
        {
            return LocalDomination(ob.Position);
        }

    }
}

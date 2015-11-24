using System.Linq;
using AutoBuddy.MainLogics;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;

namespace AutoBuddy.MyChampLogic
{
    internal class Ashe : IChampLogic
    {
        public Spell.Active Q;
        public Spell.Skillshot W, E, R;

        public Ashe()
        {
            skillSequence = new[] {2, 1, 3, 2, 2, 4, 2, 1, 2, 1, 4, 1, 1, 3, 3, 4, 3, 3};
            ShopSequence = "3340:1, 1055:1, 1:3, 1036:1, 1053:1, 1001:1, 1042:1, 3006:1, 1036:1, 1038:1, 3072:1, 1042:1, 2015:1, 1042:1, 3086:1, 3094:1, 1038:1, 3031:1, 0:4, 1042:1, 3086:1, 3085:1, 1055:2, 1037:1, 3035:1, 3036:1";
            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Skillshot(SpellSlot.W, 1200, SkillShotType.Cone);
            E = new Spell.Skillshot(SpellSlot.E, 2500, SkillShotType.Linear);
            R = new Spell.Skillshot(SpellSlot.R, 3000, SkillShotType.Linear, 250, 1600, 130)
            {
                MinimumHitChance = HitChance.Medium,
                AllowedCollisionCount = 99
            };
            Game.OnUpdate += Game_OnUpdate;
        }

        public int[] skillSequence { get; private set; }
        public LogicSelector Logic { get; set; }

        public string ShopSequence { get; private set; }

        public void Harass(AIHeroClient target)
        {
        }

        public void Survi()
        {
            if (R.IsReady() || W.IsReady())
            {
                AIHeroClient chaser =
                    EntityManager.Heroes.Enemies.FirstOrDefault(
                        chase => chase.Distance(AutoWalker.p) < 600 && chase.IsVisible());
                if (chaser != null)
                {
                    if (R.IsReady() && AutoWalker.p.HealthPercent() > 18)
                        R.Cast(chaser);
                    if (W.IsReady())
                        W.Cast(chaser);
                }
            }
        }

        public void Combo(AIHeroClient target)
        {
            if (R.IsReady() && target.HealthPercent() < 25 && AutoWalker.p.Distance(target) > 600 &&
                AutoWalker.p.Distance(target) < 1600 && target.IsVisible())
                R.Cast(target);
        }

        private void Game_OnUpdate(System.EventArgs args)
        {
            if (!R.IsReady()) return;
            AIHeroClient vic =
                EntityManager.Heroes.Enemies.FirstOrDefault(
                    v => v.IsVisible() &&
                         v.Health < AutoWalker.p.GetSpellDamage(v, SpellSlot.R) && v.Distance(AutoWalker.p) > 700 &&
                         AutoWalker.p.Distance(v) < 2500);
            if (vic == null) return;
            R.Cast(vic);
        }
    }
}
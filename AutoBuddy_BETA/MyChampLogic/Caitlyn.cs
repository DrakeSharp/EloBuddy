using System.Linq;
using System.Net.Mail;
using AutoBuddy.MainLogics;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;

namespace AutoBuddy.MyChampLogic
{
    internal class Caitlyn : IChampLogic
    {
        public Spell.Skillshot E;
        public Spell.Skillshot Q;
        public Spell.Targeted R;
        public Spell.Skillshot W;

        public Caitlyn()
        {
            skillSequence = new[] {2, 1, 1, 3, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2};
            ShopSequence = "3340:1, 1055:1, 1:3, 1036:1, 1053:1, 1001:1, 1042:1, 3006:1, 1036:1, 1038:1, 3072:1, 1042:1, 2015:1, 1042:1, 3086:1, 3094:1, 1038:1, 3031:1, 0:4, 1042:1, 3086:1, 3085:1, 1055:2, 1037:1, 3035:1, 3036:1";
            R = new Spell.Targeted(SpellSlot.R, 2000);

            Q = new Spell.Skillshot(SpellSlot.Q, 1240, SkillShotType.Linear, 250, 2000, 60);
            W = new Spell.Skillshot(SpellSlot.W, 820, SkillShotType.Circular, 500, int.MaxValue, 80);
            E = new Spell.Skillshot(SpellSlot.E, 800, SkillShotType.Linear, 250, 1600, 80)
            {
                MinimumHitChance = HitChance.Low
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
            if (E.IsReady())
            {
                AIHeroClient chaser =
                    EntityManager.Heroes.Enemies.FirstOrDefault(
                        chase => chase.Distance(AutoWalker.p) < 600 && chase.IsVisible());
                if (chaser != null)
                {
                    E.Cast(chaser);
                    return;
                }
                if (AutoWalker.p.HealthPercent() < 15)
                    E.Cast(AutoWalker.p.Position.Extend(AutoWalker.myNexus, -200).To3DWorld());
            }
        }

        public void Combo(AIHeroClient target)
        {
        }

        private void Game_OnUpdate(System.EventArgs args)
        {
            if (Orbwalker.CanAutoAttack && Logic.surviLogic.dangerValue < -20000)
            {
                AIHeroClient toShoot =
                    EntityManager.Heroes.Enemies.Where(
                        en =>
                            en.HasBuff("caitlynyordletrapinternal") &&
                            en.Distance(AutoWalker.p) < AutoWalker.p.AttackRange*2 + en.BoundingRadius)
                        .OrderBy(e => e.HealthPercent())
                        .FirstOrDefault();
                if (toShoot != null)
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, toShoot);
                }
            }

            if (!R.IsReady()) return;
            AIHeroClient vic =
                EntityManager.Heroes.Enemies.FirstOrDefault(
                    v => v.IsVisible() &&
                         v.Health < AutoWalker.p.GetSpellDamage(v, SpellSlot.R) && v.Distance(AutoWalker.p) > 670+v.BoundingRadius &&
                         AutoWalker.p.Distance(v) < 2000 && Logic.surviLogic.dangerValue < -10000);
            if (vic == null) return;
            R.Cast(vic);
        }
    }
}
using System.Linq;
using System.Runtime.InteropServices;
using Buddy_vs_Bot.MainLogics;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;

namespace Buddy_vs_Bot.MyChampLogic
{
    internal class Caitlyn : IChampLogic
    {
        public int[] skillSequence { get; private set; }
        public Spell.Skillshot Q;
        public Spell.Skillshot W;
        public Spell.Skillshot E;
        public Spell.Targeted R;
        public Caitlyn()
        {
            skillSequence = new int[] { 2, 1, 1, 3, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2 };
            {
            };
            R = new Spell.Targeted(SpellSlot.R, 2000);

            Q = new Spell.Skillshot(SpellSlot.Q, 1240, SkillShotType.Linear, 250, 2000, 60);
            W = new Spell.Skillshot(SpellSlot.W, 820, SkillShotType.Circular, 500, int.MaxValue, 80);
            E = new Spell.Skillshot(SpellSlot.E, 800, SkillShotType.Linear, 250, 1600, 80)
            {
                MinimumHitChance = HitChance.Low
            };
        }

        public void OnUpdate(LogicSelector l)
        {
            if (Orbwalker.CanAutoAttack&&l.surviLogic.dangerValue < -20000)
            {
                AIHeroClient toShoot =
                    EntityManager.Heroes.Enemies.Where(
                        en =>
                            en.HasBuff("caitlynyordletrapinternal") &&
                            en.Distance(AutoWalker.p) < AutoWalker.p.AttackRange*2 + en.BoundingRadius)
                        .OrderBy(e => e.HealthPercent)
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
                        v.Health < AutoWalker.p.GetSpellDamage(v, SpellSlot.R) && v.Distance(AutoWalker.p) > 700 &&
                        AutoWalker.p.Distance(v) < 2000&&l.surviLogic.dangerValue<-10000);
            if (vic == null) return;
            R.Cast(vic);
        }

        public void ShopLogic()
        {

            if (!AutoWalker.p.IsInShopRange()) return;
            Item trinket = new Item(ItemId.Warding_Totem_Trinket);
            Item vampiric = new Item(ItemId.Vampiric_Scepter);
            Item longsword = new Item(ItemId.Long_Sword);
            Item bf = new Item(ItemId.B_F_Sword);
            Item bt = new Item(ItemId.The_Bloodthirster);

            Item boots = new Item(ItemId.Boots_of_Speed);
            Item dagger = new Item(ItemId.Dagger);
            Item asboots = new Item(ItemId.Berserkers_Greaves);

            Item dorans = new Item(ItemId.Dorans_Blade);
            Item hpotion = new Item(ItemId.Health_Potion);

            Item run = new Item(ItemId.Runaans_Hurricane_Ranged_Only);
            Item rapid = new Item(3094);
            Item zeal = new Item(ItemId.Zeal);
            Item inf=new Item(ItemId.Infinity_Edge);
            if (!trinket.IsOwned())
            {
                trinket.Buy();
                return;
            }

            if (!inf.IsOwned() && run.IsOwned()&&bt.IsOwned()&&rapid.IsOwned())
            {
                inf.Buy();
            }
            if (!dorans.IsOwned() )
            {

                dorans.Buy();
                return;
            }
            if (ObjectManager.Player.PercentLifeStealMod < 10 && !hpotion.IsOwned() )
            {
                hpotion.Buy();
            }
            if (!bt.IsOwned() && !longsword.IsOwned())
            {
                longsword.Buy();
            }
            if (!boots.IsOwned() && !asboots.IsOwned())
            {
                boots.Buy();
            }
            if (!bt.IsOwned() && !vampiric.IsOwned() && longsword.IsOwned() )
            {
                vampiric.Buy();
            }
            if (!bt.IsOwned() && !bf.IsOwned())
            {
                bf.Buy();
            }




            if (!bt.IsOwned() && bf.IsOwned() && longsword.IsOwned() && vampiric.IsOwned())
            {
                bt.Buy();
            }


            if (!dagger.IsOwned() && !asboots.IsOwned() )
            {
                dagger.Buy();
            }

            if (!asboots.IsOwned() && boots.IsOwned() && dagger.IsOwned() )
            {
                asboots.Buy();
            }
            if (bt.IsOwned() && !zeal.IsOwned() && (!rapid.IsOwned() || !run.IsOwned())) 
            {
                zeal.Buy();
            }
            if (!rapid.IsOwned() )
            {
                rapid.Buy();
            }
            if (!run.IsOwned() )
            {

                run.Buy();
                //return;
            }

        }

        public void Harass(AIHeroClient target)
        {

        }

        public void Survi(){
            if (E.IsReady())
            {
                AIHeroClient chaser =
                    EntityManager.Heroes.Enemies.FirstOrDefault(chase => chase.Distance(AutoWalker.p) < 600 && chase.IsVisible());
                if (chaser != null)
                {
                    E.Cast(chaser);
                    return;
                }
                if (AutoWalker.p.HealthPercent < 15)
                    E.Cast(AutoWalker.p.Position.Extend(AutoWalker.myNexus, -200).To3DWorld());
            }
        }

        public void Combo(AIHeroClient target)
        {

        }
    }
}

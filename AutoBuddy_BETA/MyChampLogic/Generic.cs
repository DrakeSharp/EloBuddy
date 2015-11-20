using System.Linq;
using Buddy_vs_Bot.MainLogics;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;

namespace Buddy_vs_Bot.MyChampLogic
{
    internal class Generic : IChampLogic
    {
        public int[] skillSequence { get; private set; }
        public Spell.Active Q;
        public Spell.Skillshot W, E, R;
        private float gold;
        public Generic()
        {

            skillSequence = new int[] { 2, 1, 3, 2, 2, 4, 2, 1, 2, 1, 4, 1, 1, 3, 3, 4, 3, 3 };
            {
            };
            gold = AutoWalker.p.Gold;
        }

        public void OnUpdate(LogicSelector l)
        {
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

        public void Survi()
        {

        }

        public void Combo(AIHeroClient target)
        {

        }
    }
}

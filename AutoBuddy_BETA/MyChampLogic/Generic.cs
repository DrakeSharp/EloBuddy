using AutoBuddy.MainLogics;
using EloBuddy;
using EloBuddy.SDK;

namespace AutoBuddy.MyChampLogic
{
    internal class Generic : IChampLogic
    {
        public Spell.Active Q;
        public Spell.Skillshot W, E, R;

        public Generic()
        {
            skillSequence = new[] {2, 1, 3, 2, 2, 4, 2, 1, 2, 1, 4, 1, 1, 3, 3, 4, 3, 3};
            ShopSequence = "3340:1, 1055:1, 1:3, 1036:1, 1053:1, 1001:1, 1042:1, 3006:1, 1036:1, 1038:1, 3072:1, 1042:1, 2015:1, 1042:1, 3086:1, 3094:1, 1038:1, 3031:1, 0:4, 1042:1, 3086:1, 3085:1, 1055:2, 1037:1, 3035:1, 3036:1";
        }

        public int[] skillSequence { get; private set; }
        public LogicSelector Logic { get; set; }


        public string ShopSequence { get; private set; }

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
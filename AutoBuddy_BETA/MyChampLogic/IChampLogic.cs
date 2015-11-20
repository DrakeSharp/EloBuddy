using Buddy_vs_Bot.MainLogics;
using EloBuddy;

namespace Buddy_vs_Bot.MyChampLogic
{
    internal interface IChampLogic
    {
        int[] skillSequence { get; }
        void ShopLogic();
        void Harass(AIHeroClient target);
        void Survi();
        void OnUpdate(LogicSelector l);
        void Combo(AIHeroClient target);
    }
}

using System.Collections.Generic;
using System.Linq;
using AutoBuddy.Humanizers;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace AutoBuddy.Utilities.AutoShop
{
    internal enum ShopActionType
    {
        Buy = 1,
        Sell = 2,
        StartHpPot = 3,
        StopHpPot = 4,
        StartMpPot = 5,
        StopMpPot = 6
    }

    internal class EasyShopV2
    {
        private readonly List<BuildElement> buildElements;
        private readonly CheckBox enabled;

        public EasyShopV2(List<BuildElement> elements, CheckBox en)
        {
            enabled = en;
            buildElements = elements;
            Shopping();
        }


        private void Shopping()
        {
            List<LoLItem> myit = ItemInfo.MyItems();
            if (!enabled.CurrentValue || !ObjectManager.Player.IsInShopRange() || !buildElements.Any())
            {
                Core.DelayAction(Shopping, 300);
                return;
            }


            int currentPos = ItemInfo.GetNum(buildElements);
            if (currentPos + 2 > buildElements.Count)
            {
                Core.DelayAction(Shopping, RandGen.r.Next(400, 800));
                return;
            }

            if (buildElements.First(b => b.position == currentPos + 2).action != ShopActionType.Buy)
                foreach (
                    BuildElement buildElement in
                        buildElements.Where(b => b.position > currentPos + 1).OrderBy(b => b.position).ToList())
                {
                    if (buildElement.action == ShopActionType.Buy) break;

                    currentPos++;
                    if (currentPos + 2 > buildElements.Count)
                    {
                        Core.DelayAction(Shopping, RandGen.r.Next(400, 800));
                        return;
                    }
                }

            if (currentPos == -1)
            {
                if (!myit.Any())
                {
                    Shop.BuyItem(buildElements.First(el => el.position == 1).item.id);
                }
            }
            else if (currentPos < buildElements.Count - 1)
            {
                BuildElement b = buildElements.First(el => el.position == currentPos + 2);
                if (b.action == ShopActionType.Buy)
                    Shop.BuyItem(buildElements.First(el => el.position == currentPos + 2).item.id);
            }


            Core.DelayAction(() =>
            {
                List<BuildElement> cur = buildElements.Where(b => b.position < currentPos + 2).ToList();

                int hp = cur.Count(e => e.action == ShopActionType.StartHpPot) -
                         cur.Count(e => e.action == ShopActionType.StopHpPot);

                int mp = cur.Count(e => e.action == ShopActionType.StartMpPot) -
                         cur.Count(e => e.action == ShopActionType.StopMpPot);
                if (hp > 0 && !AutoWalker.p.InventoryItems.Any(it => it.Id == ItemId.Health_Potion))
                    Shop.BuyItem(ItemId.Health_Potion);
                if (mp > 0 && !AutoWalker.p.InventoryItems.Any(it => it.Id == ItemId.Mana_Potion))
                    Shop.BuyItem(ItemId.Mana_Potion);
            }
                , 150);

            Core.DelayAction(Shopping, RandGen.r.Next(600, 1000));
        }
    }
}
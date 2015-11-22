using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace AutoBuddy.Utilities
{
    internal enum ShopActionType
    {
        Buy=1,
        Sell=2,
        BuyHpPots=3,
        SellHpPots=4,
        BuyMpPots=5,
        SellMpPots=6
    }

    internal struct ShopAction
    {
        public int itemId;
        public ShopActionType type;
    }

    internal class EasyShop
    {
        private int hppots;
        private int mppots;
        private readonly Slider shoppointer;
        private readonly List<ShopAction> shopActions;

        public EasyShop(List<ShopAction> actions, Menu main)
        {
            shoppointer = main.Add("shoppointer", new Slider("For shop, don't touch", 0, 0, int.MaxValue));
            if (ObjectManager.Player.InventoryItems.Length<=1)
            {
                shoppointer.CurrentValue = 0;
            }
            
            shopActions = actions;
            Shopping();


        }

        public EasyShop(string actions, Menu main) : this(StringToList(actions), main){}
    
        private static List<ShopAction> StringToList(string str)
        {
            List<ShopAction> ac = new List<ShopAction>();
            foreach (string s in str.Split(','))
            {
                ShopAction a = new ShopAction();
                bool i = false;
                foreach (string ss in s.Split(':'))
                {
                    if (!i)
                    {
                        i = true;
                        a.itemId = int.Parse(ss);
                    }
                    else
                    {
                        a.type = (ShopActionType)int.Parse(ss);
                    }
                }
                ac.Add(a);
            }
            return ac;
        } 

        private void Shopping()
        {
            if (!ObjectManager.Player.IsInShopRange())
            {
                Core.DelayAction(Shopping, 500);
                return;
            }
            if (shoppointer.CurrentValue >= shopActions.Count) return;
            if (shopActions[shoppointer.CurrentValue].type == ShopActionType.Buy && ObjectManager.Player.InventoryItems.Any(inv => (int)inv.Id == shopActions[shoppointer.CurrentValue].itemId))
                shoppointer.CurrentValue++;
            Core.DelayAction(() =>
            {
                if (hppots > 0)
                {
                    InventorySlot hp =
                        ObjectManager.Player.InventoryItems.FirstOrDefault(it => it.Id == ItemId.Health_Potion);
                    if (hp == null)
                        Core.DelayAction(() => Shop.BuyItem(ItemId.Health_Potion), 100);
                }
                /*
                Chat.Print(hp == null ? hppots : hppots - hp.Stacks);
                for (int i = 0; i <= (hp == null ? hppots : hppots - hp.Stacks); i++)
                    Core.DelayAction(() => Shop.BuyItem(ItemId.Health_Potion), i*100);*/
                if (mppots > 0)
                {
                    InventorySlot mp =
                        ObjectManager.Player.InventoryItems.FirstOrDefault(it => it.Id == ItemId.Mana_Potion);
                    if (mp == null)
                        Shop.BuyItem(ItemId.Mana_Potion);
                }
                /*
                for (int i = 0; i <= (mp == null ? mppots : mppots - mp.Stacks); i++)
                    Core.DelayAction(() => Shop.BuyItem(ItemId.Mana_Potion), i*100);*/
            }, 300);
            switch (shopActions[shoppointer.CurrentValue].type)
            {
                case ShopActionType.Buy:
                    Shop.BuyItem(shopActions[shoppointer.CurrentValue].itemId);
                    break;
                case ShopActionType.Sell:
                    InventorySlot item =
                         ObjectManager.Player.InventoryItems.FirstOrDefault(it => (int)it.Id == shopActions[shoppointer.CurrentValue].itemId);
                    if (item != null)
                    {
                        Chat.Print("selling");
                        item.Sell();
                        shoppointer.CurrentValue++;
                    }
                    break;
                case ShopActionType.BuyHpPots:
                    hppots = shopActions[shoppointer.CurrentValue].itemId;
                    shoppointer.CurrentValue++;
                    break;
                case ShopActionType.BuyMpPots:
                    mppots = shopActions[shoppointer.CurrentValue].itemId;
                    shoppointer.CurrentValue++;
                    break;
                case ShopActionType.SellHpPots:
                    if (ObjectManager.Player.InventoryItems.Any(it => it.Id == ItemId.Health_Potion))
                    {
                        InventorySlot hp = ObjectManager.Player.InventoryItems.First(it => it.Id == ItemId.Health_Potion);
                        //for (int i = 0; i < (hp == null ? hppots : hp.Stacks); i++)
                            Core.DelayAction(() => Shop.SellItem(ItemId.Health_Potion), 100);
                        shoppointer.CurrentValue++;
                    }
                    hppots = 0;
                    break;
                case ShopActionType.SellMpPots:
                    if (ObjectManager.Player.InventoryItems.Any(it => it.Id == ItemId.Mana_Potion))
                    {
                        //InventorySlot mp = ObjectManager.Player.InventoryItems.First(it => it.Id == ItemId.Mana_Potion);
                        //for (int i = 0; i < (mp == null ? mppots : mp.Stacks); i++)
                            Core.DelayAction(() => Shop.SellItem(ItemId.Mana_Potion), 20);
                        shoppointer.CurrentValue++;
                    }
                    mppots = 0;
                    break;

            }

            Core.DelayAction(Shopping, 1000);



        }



    }
}

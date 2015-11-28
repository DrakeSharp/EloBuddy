using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EloBuddy;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace AutoBuddy.Utilities.AutoShop
{
    internal class BuildCreator
    {

        private readonly List<BuildElement> myBuild;
        private readonly Menu menu;
        private readonly Label l;
        private readonly string buildFile;
        private readonly EasyShopV2 shop;
        private readonly CheckBox enabled;
        private readonly PropertyInfo property;
        private readonly string sugBuild;


        public BuildCreator(Menu menu, string dir)
        {
            sugBuild = string.Empty;
            property = typeof(CheckBox).GetProperty("Position");
            buildFile = Path.Combine(dir + "\\" + AutoWalker.p.ChampionName + "-" + Game.MapId + ".txt");
            l = new Label("Shopping list for " + Game.MapId);
            enabled = new CheckBox("Auto buy enabled", true);
            myBuild = new List<BuildElement>();

            this.menu = menu.AddSubMenu("AutoShop: " + AutoWalker.p.ChampionName, "AB_SHOP_" + AutoWalker.p.ChampionName);
            this.menu.Add("eeewgrververv", l);
            this.menu.Add(AutoWalker.p.ChampionName + "enabled", enabled);
            LoadBuild();
            shop=new EasyShopV2(myBuild, enabled);
            Chat.OnInput += Chat_OnInput;
            Drawing.OnEndScene += Drawing_OnEndScene;

        }
        public BuildCreator(Menu menu, string dir, string build)
        {
            sugBuild = build;
            property = typeof(CheckBox).GetProperty("Position");
            buildFile = Path.Combine(dir + "\\" + AutoWalker.p.ChampionName + "-" + Game.MapId + ".txt");
            l = new Label("Shopping list for " + Game.MapId);
            enabled = new CheckBox("Auto buy enabled", true);
            myBuild = new List<BuildElement>();

            this.menu = menu.AddSubMenu("AutoShop: " + AutoWalker.p.ChampionName, "AB_SHOP_" + AutoWalker.p.ChampionName);
            this.menu.Add("eeewgrververv", l);
            this.menu.Add(AutoWalker.p.ChampionName + "enabled", enabled);
            LoadBuild();
            shop = new EasyShopV2(myBuild, enabled);
            Chat.OnInput += Chat_OnInput;
            Drawing.OnEndScene += Drawing_OnEndScene;

        }



        private void AddElement(LoLItem it, ShopActionType ty)
        {

            if (ty != ShopActionType.Buy)
            {
                int hp = myBuild.Count(e => e.action == ShopActionType.StartHpPot) -
                         myBuild.Count(e => e.action == ShopActionType.StopHpPot);
                int mp = myBuild.Count(e => e.action == ShopActionType.StartMpPot) -
                         myBuild.Count(e => e.action == ShopActionType.StopMpPot);
            if (ty == ShopActionType.StartHpPot && hp!=0) return;
            if (ty == ShopActionType.StartMpPot && mp!=0) return;
            if (ty == ShopActionType.StopHpPot && hp==0) return;
            if (ty == ShopActionType.StopMpPot && mp==0) return;
            }

            BuildElement b = new BuildElement(this, menu, it, myBuild.Any() ? myBuild.Max(a => a.position) + 1 : 1, ty);

            List<LoLItem> c = new List<LoLItem>();
            ItemInfo.InventorySimulator(myBuild, c);
            b.cost = ItemInfo.InventorySimulator(new List<BuildElement> { b }, c);
            b.freeSlots = 7 - c.Count;
            b.updateText();
            if (b.freeSlots == -1)
            {
                Chat.Print("Couldn't add "+it+", inventory is full.");
                b.Remove(menu);
            }
            else
                myBuild.Add(b);
        }

        private void LoadBuild()
        {
            if (!File.Exists(buildFile))
            {
                if (!sugBuild.Equals(string.Empty))
                {
                    LoadInternalBuild();
                }
                return;
            }
            try
            {
                string s = File.ReadAllText(buildFile);
                if (s.Equals(string.Empty))
                {
                    Chat.Print("AutoBuddy: the build is empty.");
                    LoadInternalBuild();
                    return;
                }
                foreach (ItemAction ac in DeserializeBuild(s))
                {
                    AddElement(ItemInfo.GetItemByID(ac.item), ac.t);
                }
            }
            catch (Exception e)
            {
                Chat.Print("AutoBuddy: couldn't load the build.");
                LoadInternalBuild();
                Console.WriteLine(e.Message);
            }
        }

        private void LoadInternalBuild()
        {
            try
            {
                if (sugBuild.Equals(string.Empty))
                {
                    Chat.Print("AutoBuddy: internal build is empty.");
                    return;
                }
                foreach (ItemAction ac in DeserializeBuild(sugBuild))
                {
                    AddElement(ItemInfo.GetItemByID(ac.item), ac.t);
                }
            }
            catch (Exception e)
            {
                Chat.Print("AutoBuddy: internal build load failed.");
                Console.WriteLine(e.Message);
            }
            Chat.Print("AutoBuddy: loaded internal build(change it if you want!).");
        }

        private void SaveBuild()
        {
            File.WriteAllText(buildFile, SerializeBuild());
        }

        private string SerializeBuild()
        {
            string s = string.Empty;
            foreach (BuildElement el in myBuild.OrderBy(el => el.position))
            {
                s += el.item.id + ":" + el.action + ",";
            }
            return s.Equals(string.Empty)?s:s.Substring(0, s.Length - 1);
        }

        private struct ItemAction
        {
            public ShopActionType t;
            public int item;
        }
        private IEnumerable<ItemAction> DeserializeBuild(string serialized)
        {
            List<ItemAction> b = new List<ItemAction>();
            foreach (string s in serialized.Split(','))
            {
                ItemAction ac = new ItemAction { item = -1 };
                foreach (string s2 in s.Split(':'))
                {
                    if (ac.item == -1)
                        ac.item = int.Parse(s2);
                    else
                        ac.t = (ShopActionType)Enum.Parse(typeof(ShopActionType), s2, true);

                }
                b.Add(ac);
            }
            return b;
        }
        private void Drawing_OnEndScene(EventArgs args)
        {
            if (!MainMenu.IsVisible) return;
            property.GetSetMethod(true).Invoke(enabled, new object[] { l.Position + new Vector2(433, 0) });
            foreach (BuildElement ele in myBuild)
            {
                ele.UpdatePos(new Vector2(l.Position.X, l.Position.Y+10));
            }
        }

        public void MoveUp(int index)
        {
            if (index <= 2) return;
            BuildElement th = myBuild.First(ele => ele.position == index);
            BuildElement up = myBuild.First(ele => ele.position == index - 1);
            th.position--;
            up.position++;

            foreach (BuildElement el in myBuild.OrderBy(b => b.position))
            {


                List<LoLItem> c = new List<LoLItem>();
                ItemInfo.InventorySimulator(myBuild, c, el.position - 1);
                el.cost = ItemInfo.InventorySimulator(new List<BuildElement> { el }, c);
                el.freeSlots = 7 - c.Count;
                el.updateText();
            }
            SaveBuild();
        }
        public void MoveDown(int index)
        {
            if (index == myBuild.Count||index==2) return;
            BuildElement th = myBuild.First(ele => ele.position == index);
            BuildElement dn = myBuild.First(ele => ele.position == index + 1);
            th.position++;
            dn.position--;

            SaveBuild();
        }
        public bool Remove(int index)
        {
            if (myBuild.Count>1&&index == 1) return false;
            BuildElement th = myBuild.First(ele => ele.position == index);
            myBuild.Remove(th);
            th.Remove(menu);
            foreach (BuildElement el in myBuild.OrderBy(b => b.position).Where(b => b.position > index))
            {
                el.position--;


                List<LoLItem> c = new List<LoLItem>();
                ItemInfo.InventorySimulator(myBuild, c, el.position - 1);
                el.cost = ItemInfo.InventorySimulator(new List<BuildElement> { el }, c);
                el.freeSlots = 7 - c.Count;
                el.updateText();
            }




            SaveBuild();
            return true;
        }

        private void Chat_OnInput(ChatInputEventArgs args)
        {

            if (args.Input.ToLower().StartsWith("/b "))
            {
                args.Process = false;
                string itemName = args.Input.Substring(2);
                LoLItem i = ItemInfo.FindBestItem(itemName);
                Chat.Print("Buy " + i.name);

                if (myBuild.Count == 0 && !i.groups.Equals("RelicBase"))
                {
                    AddElement(ItemInfo.GetItemByID(3340), ShopActionType.Buy);
                    Chat.Print("Added also warding totem.");
                }
                AddElement(i, ShopActionType.Buy);
                SaveBuild();
            }
            else if (args.Input.ToLower().Equals("/buyhp"))
            {
                if (myBuild.Count == 0)
                {
                    AddElement(ItemInfo.GetItemByID(3340), ShopActionType.Buy);
                    Chat.Print("Added also warding totem.");
                }
                AddElement(ItemInfo.GetItemByID(2003), ShopActionType.StartHpPot);
                SaveBuild();
                args.Process = false;
            }
            else if (args.Input.ToLower().Equals("/buymp"))
            {
                if (myBuild.Count == 0)
                {
                    AddElement(ItemInfo.GetItemByID(3340), ShopActionType.Buy);
                    Chat.Print("Added also warding totem.");
                }
                AddElement(ItemInfo.GetItemByID(2004), ShopActionType.StartMpPot);
                SaveBuild();
                args.Process = false;
            }
            else if (args.Input.ToLower().Equals("/stophp"))
            {
                if (myBuild.Count == 0)
                {
                    AddElement(ItemInfo.GetItemByID(3340), ShopActionType.Buy);
                    Chat.Print("Added also warding totem.");
                }
                AddElement(ItemInfo.GetItemByID(2003), ShopActionType.StopHpPot);
                SaveBuild();
                args.Process = false;
            }
            else if (args.Input.ToLower().Equals("/stopmp"))
            {
                if (myBuild.Count == 0)
                {
                    AddElement(ItemInfo.GetItemByID(3340), ShopActionType.Buy);
                    Chat.Print("Added also warding totem.");
                }
                AddElement(ItemInfo.GetItemByID(2004), ShopActionType.StopMpPot);
                SaveBuild();
                args.Process = false;
            }
        }



    }
}

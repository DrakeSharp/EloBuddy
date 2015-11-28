using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using AutoBuddy.Properties;
using EloBuddy;
using Newtonsoft.Json.Linq;

namespace AutoBuddy.Utilities.AutoShop
{
    internal static class ItemInfo
    {
        public static readonly LoLItem[] itemDB;
        public static readonly LoLItem[] avItemDB;

        static ItemInfo()
        {
            List<LoLItem> all = new List<LoLItem>();
            List<LoLItem> av = new List<LoLItem>();
            foreach (LoLItem loLItem in ParseItems())
            {

                all.Add(loLItem);
                if (loLItem.purchasable && loLItem.maps.Contains((int)Game.MapId) && (loLItem.requiredChampion == string.Empty || loLItem.requiredChampion == AutoWalker.p.ChampionName))
                    av.Add(loLItem);
            }
            itemDB = all.ToArray();
            avItemDB = av.ToArray();
        }

        public static LoLItem FindBestItem(this List<LoLItem> items, string name)
        {
            return items.OrderByDescending(it => it.name.Match(name)).First();
        }

        public static LoLItem FindBestItem(this LoLItem[] items, string name)
        {
            return items.OrderByDescending(it => it.name.Match(name)).First();
        }
        public static LoLItem FindBestItem(string name)
        {
            return avItemDB.OrderByDescending(it => it.name.Match(name)).First();
        }

        public static LoLItem FindItemByID(this LoLItem[] items, int id)
        {
            return items.OrderByDescending(it => it.id == id).First();
        }

        public static LoLItem FindItemByID(this List<LoLItem> items, int id)
        {
            return items.OrderByDescending(it => it.id == id).First();
        }

        public static List<LoLItem> MyItems()
        {
            List<LoLItem>l= AutoWalker.p.InventoryItems.Select(s => itemDB.FindItemByID((int)s.Id)).ToList();
            l.Remove(l.FirstOrDefault(le => le.id == 1411));//TODO !! Remove this when eb or rito will fix it
            return l;
        }

        public static LoLItem GetItemByID(int id)
        {
            return itemDB.First(it => it.id == id);
        }


        public static List<LoLItem> ParseItems()
        {
            JObject data = JObject.Parse(Resources.item);
            List<LoLItem> loLItems = new List<LoLItem>();
            foreach (JToken token in data.GetValue("data"))
            {
                JToken t = token.First;

                List<int> maps = new List<int>();
                if ((bool)t["maps"]["1"]) maps.Add(1);
                if ((bool)t["maps"]["8"]) maps.Add(8);
                if ((bool)t["maps"]["10"]) maps.Add(10);
                if ((bool)t["maps"]["11"]) maps.Add(11);
                if ((bool)t["maps"]["12"]) maps.Add(12);
                if ((bool)t["maps"]["14"]) maps.Add(14);

                List<int> fromItems = new List<int>();
                if (t["from"] != null)
                    fromItems.AddRange(t["from"].Select(tok => (int)tok));

                List<int> toItems = new List<int>();
                if (t["to"] != null)
                    toItems.AddRange(t["to"].Select(tok => (int)tok));

                List<string> tags = new List<string>();
                if (t["tags"] != null)
                    tags.AddRange(t["tags"].Select(tok => tok.ToString()));

                loLItems.Add(new LoLItem(t["name"].ToString(), t["description"].ToString(), t["sanitizedDescription"].ToString(), t["plaintext"] == null ? string.Empty : t["plaintext"].ToString(),
                    (int)t["id"], (int)t["gold"]["base"], (int)t["gold"]["total"], (int)t["gold"]["sell"], (bool)t["gold"]["purchasable"],
                    t["requiredChampion"] == null ? string.Empty : t["requiredChampion"].ToString(), maps.ToArray(), fromItems.ToArray(), toItems.ToArray(), t["depth"] == null ? -1 : (int)t["depth"], tags.ToArray(), t["colloq"] == null ? string.Empty : t["colloq"].ToString(), t["group"] == null ? string.Empty : t["group"].ToString()));

            }
            return loLItems;
        }

        public static int InventorySimulator(List<BuildElement> elements, List<LoLItem> playerInv, int num = int.MaxValue)
        {
            int n = 0;
            int gold = 0;
            foreach (BuildElement el in elements.OrderBy(el => el.position))
            {
                if (n >= num)
                    return gold;
                n++;
                if (el.action == ShopActionType.Buy)
                {
                    gold+=BuyItemSim(playerInv, el.item);
                    playerInv.Add(el.item);
                }
                else if (el.action == ShopActionType.StartHpPot)
                {
                    gold += BuyItemSim(playerInv, el.item);
                    if(playerInv.FirstOrDefault(ii=>ii.id==(int)ItemId.Health_Potion)==null)
                        playerInv.Add(el.item);
                }
                else if (el.action == ShopActionType.StartMpPot)
                {
                    gold += BuyItemSim(playerInv, el.item);
                    if (playerInv.FirstOrDefault(ii => ii.id == (int)ItemId.Mana_Potion) == null)
                        playerInv.Add(el.item);
                }
                else if (el.action == ShopActionType.StopMpPot)
                {
                    gold += BuyItemSim(playerInv, el.item);
                    playerInv.Remove(el.item);
                }
                else if (el.action == ShopActionType.StopHpPot)
                {
                    playerInv.Remove(el.item);
                }
            }
            return gold;
        }

        public static int GetNum(List<BuildElement> elements)
        {
            int n = 0;
            List<LoLItem> myItems = MyItems();
            List<LoLItem> virtInv = new List<LoLItem>();
            foreach (BuildElement el in elements.OrderBy(el => el.position))
            {
                if (el.action == ShopActionType.Buy)
                {
                    BuyItemSim(virtInv, el.item);
                    virtInv.Add(el.item);
                    if (virtInv.Equal(myItems)) return n;
                }
                
                n++;
            }
            return -1;
        }

        public static bool Equal(this List<LoLItem> listOne, List<LoLItem> listTwo, bool ignorePotions = true)
        {
            if (!listOne.Any() && !listTwo.Any()) return true;
            List<LoLItem> lTwo = new List<LoLItem>(listTwo);
            foreach (LoLItem itOne in listOne)
            {
                bool cont = false;
                if (itOne.id == (int)ItemId.Health_Potion || itOne.id == (int)ItemId.Mana_Potion) continue;
                foreach (LoLItem itTwo in lTwo)
                {
                    if (itOne.id == (int)ItemId.Health_Potion || itOne.id == (int)ItemId.Mana_Potion) continue;

                    if (itOne.id == itTwo.id)
                    {
                        lTwo.Remove(itTwo);
                        cont = true;
                        break;
                    }

                }
                if (!cont) return false;
            }
            
            return !lTwo.Any(l => l.id != (int) ItemId.Health_Potion && l.id != (int) ItemId.Mana_Potion);
        }



        public static int BuyItemSim(List<LoLItem> inventory, LoLItem item, bool root=true)
        {
            if (!root&&inventory.Any(it => it.id == item.id))
            {
                inventory.Remove(inventory.First(it => it.id == item.id));
                return 0;
            }
            if (item.fromItems.Length == 0)
            {
                return item.baseGold;
            }
            int gold = item.baseGold + item.fromItems.Sum(fromItemID => BuyItemSim(inventory, itemDB.First(it => it.id == fromItemID), false));
            return gold;
        }
    }
}

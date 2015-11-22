using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using AutoBuddy.Humanizers;
using AutoBuddy.MainLogics;
using AutoBuddy.MyChampLogic;
using AutoBuddy.Utilities;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using Version = System.Version;

namespace AutoBuddy
{
    internal class Program
    {
        private static Menu menu;
        private static IChampLogic myChamp;
        public static SkillLevelUp LevelUp { get; private set; }
        private static EasyShop easyShop;
        public static LogicSelector Logic { get; private set; }

        public static void Main()
        {
            Hacks.RenderWatermark = false;
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }


        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Chat.Print("AutoBuddy will start in 5 seconds.");
            Core.DelayAction(Start, 5000);
            menu = MainMenu.AddMenu("AUTOBUDDY", "AB");
            menu.Add("autolvl", new CheckBox("Disable built-in skill leveler(press f5 after)", false));
            menu.Add("sep1", new Separator(1));
            menu.Add("mid", new CheckBox("Try to go mid, will leave if other player is on mid", true));
            menu.Add("lane", new Slider("Lane:   1:Auto | 2:Top | 3:Mid | 4:Bot", 1, 1, 4));
            menu.Add("sep2", new Separator(250));
            menu.Add("reselectlane", new CheckBox("Reselect lane", false));
            menu.Add("debuginfo", new CheckBox("Draw debug info(press f5 after)", true));
            menu.Add("l1", new Label("By Christian Brutal Sniper"));
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            menu.Add("l2",
                new Label("Version " + v.Major + "." + v.Minor + " Build time: " + v.Build%100 + " " +
                          CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(v.Build/100) + " " +
                          (v.Revision/100).ToString().PadLeft(2, '0') + ":" +
                          (v.Revision%100).ToString().PadLeft(2, '0')));


        }

        private static void Start()
        {
            RandGen.Start();
            bool generic = false;
            if (ObjectManager.Player.Hero == Champion.Ashe)
            {
                myChamp = new Ashe();
            }
            else if (ObjectManager.Player.Hero == Champion.Caitlyn)
            {
                myChamp = new Caitlyn();
            }
            else
            {
                generic = true;
                myChamp = new Generic();
            }
            if (!generic)
                easyShop = new EasyShop(myChamp.ShopSequence, menu);
                
            else
            {
                myChamp = new Generic();
                if (MainMenu.GetMenu("AB_" + ObjectManager.Player.ChampionName)!=null&&MainMenu.GetMenu("AB_" + ObjectManager.Player.ChampionName).Get<Label>("shopSequence") != null)
                {
                    Chat.Print("Autobuddy: Loaded shop plugin for " + ObjectManager.Player.ChampionName);
                    easyShop = new EasyShop(
                        MainMenu.GetMenu("AB_" + ObjectManager.Player.ChampionName)
                            .Get<Label>("shopSequence")
                            .DisplayName, menu);
                }
                else
                {
                    Chat.Print("Autobuddy shop and auto skill lvl: no plugins detected for " +
                               ObjectManager.Player.ChampionName + ", using generic ADC build");
                    easyShop = new EasyShop(myChamp.ShopSequence, menu);
                }
                
            }
            if (!menu.Get<CheckBox>("autolvl").CurrentValue)
                LevelUp = new SkillLevelUp(myChamp);
            Logic = new LogicSelector(myChamp);
        }

    }
}
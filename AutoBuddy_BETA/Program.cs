using System;
using System.Globalization;
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
using IChampLogic = AutoBuddy.MyChampLogic.IChampLogic;
using Version = System.Version;

namespace AutoBuddy
{
    internal class Program
    {
        private static Menu _menu;
        private static IChampLogic _myChamp;
        public static SkillLevelUp LevelUp { get; private set; }
        private static EasyShop _easyShop;
        public static LogicSelector Logic { get; private set; }

        public static void Main()
        {
            Hacks.RenderWatermark = false;
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }


        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Chat.Print("AutoBuddy will start in 5 seconds. ");
            Core.DelayAction(Start, 5000);
            _menu = MainMenu.AddMenu("AUTOBUDDY", "AB");
            _menu.Add("autolvl", new CheckBox("Disable built-in skill leveler(press f5 after)", false));
            _menu.Add("sep1", new Separator(1));
            _menu.Add("mid", new CheckBox("Try to go mid, will leave if other player is on mid", true));
            _menu.Add("lane", new Slider("Lane:   1:Auto | 2:Top | 3:Mid | 4:Bot", 1, 1, 4));
            _menu.Add("sep2", new Separator(250));
            _menu.Add("reselectlane", new CheckBox("Reselect lane", false));
            _menu.Add("debuginfo", new CheckBox("Draw debug info(press f5 after)", true));
            _menu.Add("l1", new Label("By Christian Brutal Sniper"));
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            _menu.Add("l2",
                new Label("Version " + v.Major + "." + v.Minor + " Build time: " + v.Build%100 + " " +
                          CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(v.Build/100) + " " +
                          (v.Revision/100).ToString().PadLeft(2, '0') + ":" +
                          (v.Revision%100).ToString().PadLeft(2, '0')));

            
        }


        private static void Start()
        {
            RandGen.Start();
            bool generic = false;
            switch (ObjectManager.Player.Hero)
            {
                case Champion.Ashe:
                    _myChamp = new Ashe();
                    break;
                case Champion.Caitlyn:
                    _myChamp = new Caitlyn();
                    break;
                default:
                    generic = true;
                    _myChamp = new Generic();
                    break;
                case Champion.Ezreal:
                    _myChamp = new Ezreal();
                    break;
            }
            if (!generic)
                _easyShop = new EasyShop(_myChamp.ShopSequence, _menu);
                
            else
            {
                _myChamp = new Generic();
                if (MainMenu.GetMenu("AB_" + ObjectManager.Player.ChampionName)!=null&&MainMenu.GetMenu("AB_" + ObjectManager.Player.ChampionName).Get<Label>("shopSequence") != null)
                {
                    Chat.Print("Autobuddy: Loaded shop plugin for " + ObjectManager.Player.ChampionName);
                    _easyShop = new EasyShop(
                        MainMenu.GetMenu("AB_" + ObjectManager.Player.ChampionName)
                            .Get<Label>("shopSequence")
                            .DisplayName, _menu);
                }
                else
                {
                    Chat.Print("Autobuddy shop and auto skill lvl: no plugins detected for " +
                               ObjectManager.Player.ChampionName + ", using generic ADC build");
                    _easyShop = new EasyShop(_myChamp.ShopSequence, _menu);
                }
                
            }
            if (!_menu.Get<CheckBox>("autolvl").CurrentValue)
                LevelUp = new SkillLevelUp(_myChamp);
            Logic = new LogicSelector(_myChamp);
        }

    }
}
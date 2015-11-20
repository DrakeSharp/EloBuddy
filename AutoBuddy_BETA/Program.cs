using System;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using Buddy_vs_Bot.MainLogics;
using Buddy_vs_Bot.MyChampLogic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using Version = System.Version;

namespace Buddy_vs_Bot
{
    internal class Program
    {
        private static Menu menu;
        public static SkillLevelUp LevelUp { get; private set; }
        public static LogicSelector Logic { get; private set; }
        private static IChampLogic myChamp;

        public static void Main()
        {
            Hacks.RenderWatermark = false;
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
            
            

        }

        // ReSharper disable once UnusedMember.Local
        private static void Drawing_OnDraw(EventArgs args)
        {

        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Chat.Print("AutoBuddy will start in 6 seconds.");
            Core.DelayAction(Start, 6000);
            menu = MainMenu.AddMenu("AUTOBUDDY", "AB");
            menu.Add("autolvl", new CheckBox("Disable built-in skill leveler(press f5 after)", false));
            menu.Add("sep1", new Separator(1));
            menu.Add("mid", new CheckBox("Try to go mid, will leave if other player is on mid", true));
            menu.Add("lane", new Slider("Lane:   1:Auto | 2:Top | 3:Mid | 4:Bot", 1, 1, 4));
            menu.Add("sep2", new Separator(250));
            menu.Add("reselectlane", new CheckBox("Reselect lane", false));
            menu.Add("debuginfo", new CheckBox("Draw debug info(press f5 after)", true));
            menu.Add("l1", new Label("By Christian Brutal Sniper"));
            Version v=Assembly.GetExecutingAssembly().GetName().Version;
            menu.Add("l2", new Label(("Version " + v.Major + "." + v.Minor + " Build time: " + v.Build % 100 + " " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(v.Build / 100)) + " " + (v.Revision / 100).ToString().PadLeft(2, '0') + ":" + (v.Revision % 100).ToString().PadLeft(2, '0')));
        }

        private static void Start()
        {
            RandGen.Start();
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
                myChamp = new Generic();
            }
            if(!menu.Get<CheckBox>("autolvl").CurrentValue)
                LevelUp = new SkillLevelUp(myChamp);
            AutoShop();
            Logic = new LogicSelector(myChamp);
        }
        private static void AutoShop()
        {   myChamp.ShopLogic();
            Core.DelayAction(AutoShop, RandGen.r.Next(300, 800));
        }
    }
}

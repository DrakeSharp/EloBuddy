using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Buddy_vs_Bot.MainLogics;
using Buddy_vs_Bot.MyChampLogic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

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
            
            Drawing.OnDraw += Drawing_OnDraw;
            

        }

        // ReSharper disable once UnusedMember.Local
        private static void Drawing_OnDraw(EventArgs args)
        {

            //Drawing.DrawCircle(AutoWalker.p.Position.Extend(Game.CursorPos, 400).To3DWorld(), 50, Color.Blue);
            //Drawing.DrawCircle(AutoWalker.p.Position.Extend(Game.CursorPos, 400).To3DWorld().RotateAround(AutoWalker.p.Position, 1.570f), 50, Color.Coral);
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Chat.Print("AutoBuddy will start in 6 seconds.");
            Core.DelayAction(Start, 6000);
            menu = MainMenu.AddMenu("AUTOBUDDY", "AB");
            menu.Add("autolvl", new CheckBox("Disable built-in skill leveler", false));
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

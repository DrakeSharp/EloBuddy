using Buddy_vs_Bot.MyChampLogic;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;
using Color = System.Drawing.Color;

namespace Buddy_vs_Bot.MainLogics
{
    internal class LogicSelector
    {
        internal enum MainLogics
        {
            PushLogic,
            RecallLogic,
            LoadLogic,
            SurviLogic,
            CombatLogic,
            Nothing
        }

        public IChampLogic myChamp;
        public MainLogics current { get; set; }
        public readonly Push pushLogic;
        public readonly Load loadLogic;
        public readonly Recall recallLogic;
        public readonly Survi surviLogic;
        public readonly Combat combatLogic;
        public readonly Surrender surrender;
        public readonly LocalAwareness localAwareness;
        public bool saveMylife;

        public LogicSelector(IChampLogic my)
        {
            myChamp = my;
            current = MainLogics.Nothing;
            surviLogic = new Survi(this);
            recallLogic = new Recall(this);
            pushLogic = new Push(this);
            loadLogic = new Load(this);
            combatLogic = new Combat(this);
            surrender=new Surrender();
            Core.DelayAction(()=>loadLogic.SetLane(), 1000);
            localAwareness=new LocalAwareness();
            
            Drawing.OnEndScene += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Core.DelayAction(Watchdog, 3000);

        }

        void Game_OnUpdate(System.EventArgs args)
        {
            myChamp.OnUpdate(this);
        }

        void Drawing_OnDraw(System.EventArgs args)
        {
            Drawing.DrawText(250, 85, Color.Gold, current.ToString());
            Vector2 v = Game.CursorPos.WorldToScreen();
            Drawing.DrawText(v.X, v.Y-20, Color.Gold, localAwareness.LocalDomination(Game.CursorPos)+" ");
        }

        public MainLogics SetLogic(MainLogics newlogic)
        {
            if (saveMylife) return current;
            if (newlogic != MainLogics.PushLogic)
                pushLogic.Deactivate();
            MainLogics old = current;
            switch (current)
            {

                case MainLogics.SurviLogic:
                    surviLogic.Deactivate();
                    break;

                case MainLogics.RecallLogic:
                    recallLogic.Deactivate();
                    break;
                case MainLogics.CombatLogic:
                    combatLogic.Deactivate();
                    break;
            }



            switch (newlogic)
            {
                case MainLogics.PushLogic:
                    pushLogic.Activate();
                    break;
                case MainLogics.LoadLogic:
                    loadLogic.Activate();
                    break;
                case MainLogics.SurviLogic:
                    surviLogic.Activate();

                    break;
                case MainLogics.RecallLogic:
                    recallLogic.Activate();
                    break;
                case MainLogics.CombatLogic:
                    combatLogic.Activate();
                    break;
            }


            current = newlogic;
            return old;
        }

        private void Watchdog()
        {
            Core.DelayAction(Watchdog, 500);
            if (current == MainLogics.Nothing && !loadLogic.waiting)
            {
                Chat.Print("Hang detected");
                loadLogic.SetLane();
            }

        }

    }
}

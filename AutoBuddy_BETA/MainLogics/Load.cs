using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;

namespace Buddy_vs_Bot.MainLogics
{
    internal class Load
    {
        public bool waiting = false;
        private readonly LogicSelector currentLogic;
        private const float waitTime = 40;
        private readonly float startTime;
        private string status=" ";
        public Load(LogicSelector c)
        {

            currentLogic = c;
            startTime = Game.Time + waitTime + RandGen.r.NextFloat(-10, 20);
            Drawing.OnDraw += Drawing_OnDraw;

        }

        void Drawing_OnDraw(System.EventArgs args)
        {
            Drawing.DrawText(250, 70, System.Drawing.Color.Gold, "Lane selector status: " + status);
        }

        public void Activate()
        {

        }
        public void SetLane()
        {

            if (ObjectManager.Get<Obj_AI_Turret>().Count() == 24)
            {
                if (AutoWalker.p.Gold < 550)
                {
                    Vector3 p =
                        ObjectManager.Get<Obj_AI_Turret>()
                            .First(tur => tur.IsAlly && tur.Name.EndsWith("C_05_A"))
                            .Position;
                
                Core.DelayAction(
                    () =>
                        TacticalMap.SendPing(PingCategory.OnMyWay,
                            new Vector3(p.X + RandGen.r.NextFloat(-300, 300), p.Y + RandGen.r.NextFloat(-300, 300),
                                p.Z)), RandGen.r.Next(3000));

                Core.DelayAction(() => Chat.Say("mid"), RandGen.r.Next(200, 1000));
                AutoWalker.WalkTo(p);
            }
            CanSelectLane();
            }
            else
                SelectMostPushedLane();
        }
        public void Deactivate()
        {

        }

        public void CanSelectLane()
        {
            waiting = true;
            status="searching for free lane, time left "+(startTime-Game.Time);
            if (Game.Time > startTime || GetChampLanes().All(cl => cl.lane != Lane.Unknown))
            {
                waiting = false;
                SelectLane();
            }
            else
                Core.DelayAction(CanSelectLane, 500);
        }

        public void SelectMostPushedLane()
        {
            status = "selected most pushed lane";
            Obj_HQ nmyNexus = ObjectManager.Get<Obj_HQ>().First(hq => hq.IsEnemy);

            Obj_AI_Minion andrzej =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(min =>min.Name.Contains("Minion")&& min.IsAlly&&min.Health>0)
                    .OrderBy(min => min.Distance(nmyNexus))
                    .First();

            Obj_AI_Base ally =
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(tur => tur.IsAlly &&tur.Health>0&& tur.GetLane() == andrzej.GetLane())
                    .OrderBy(tur => tur.Distance(andrzej))
                    .FirstOrDefault();
            if (ally == null)
            {
                ally =
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(tur => tur.Health > 0 && tur.IsAlly
                && tur.GetLane() == Lane.HQ)
                    .OrderBy(tur => tur.Distance(andrzej))
                    .FirstOrDefault();
            }
            if (ally == null)
            {
                ally = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(tur => tur.IsAlly && tur.GetLane() == Lane.Spawn);
            }

            Obj_AI_Base enemy =
    ObjectManager.Get<Obj_AI_Turret>()
        .Where(tur => tur.IsEnemy && tur.Health > 0 && tur.GetLane() == andrzej.GetLane())
        .OrderBy(tur => tur.Distance(andrzej))
        .FirstOrDefault();
            if (enemy == null)
            {
                enemy =
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(tur => tur.Health > 0 && tur.IsEnemy
                && tur.GetLane() == Lane.HQ)
                    .OrderBy(tur => tur.Distance(andrzej))
                    .FirstOrDefault();
            }
            if (enemy == null)
            {
                enemy = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(tur => tur.IsEnemy && tur.GetLane() == Lane.Spawn);
            }

            currentLogic.pushLogic.Reset(ally, enemy, andrzej.GetLane());

        }
        private void SelectLane()
        {
            status = "selected free lane";
            List<ChampLane> list = GetChampLanes();
            if (list.All(cl => cl.lane != Lane.Mid))
            {
                currentLogic.pushLogic.Reset(ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsAlly && tur.Name.EndsWith("C_05_A")), ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsEnemy && tur.Name.EndsWith("C_05_A")), Lane.Mid);
                return;
            }
            if (list.Count(cl => cl.lane == Lane.Bot) < 2)
            {
                currentLogic.pushLogic.Reset(ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsAlly && tur.Name.EndsWith("R_03_A")), ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsEnemy && tur.Name.EndsWith("R_03_A")), Lane.Bot);
                return;
            }
            if (list.Count(cl => cl.lane == Lane.Top) < 2)
            {
                currentLogic.pushLogic.Reset(ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsAlly && tur.Name.EndsWith("L_03_A")), ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsEnemy && tur.Name.EndsWith("L_03_A")), Lane.Top);
            }
        }

        private static List<ChampLane> GetChampLanes(float maxDistance = 2000, float maxDistanceFront = 4000)
        {
            Obj_AI_Turret top1 = ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsAlly && tur.Name.EndsWith("L_03_A"));
            Obj_AI_Turret top2 = ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsAlly && tur.Name.EndsWith("L_02_A"));
            Obj_AI_Turret mid1 = ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsAlly && tur.Name.EndsWith("C_05_A"));
            Obj_AI_Turret mid2 = ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsAlly && tur.Name.EndsWith("C_04_A"));
            Obj_AI_Turret bot1 = ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsAlly && tur.Name.EndsWith("R_03_A"));
            Obj_AI_Turret bot2 = ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsAlly && tur.Name.EndsWith("R_02_A"));

            List<ChampLane> ret = new List<ChampLane>();

            foreach (AIHeroClient h in EntityManager.Heroes.Allies.Where(hero => hero.IsAlly && !hero.IsMe))
            {
                Lane lane = Lane.Unknown;
                if (h.Distance(top1) < maxDistanceFront || h.Distance(top2) < maxDistance) lane = Lane.Top;
                if (h.Distance(mid1) < maxDistanceFront || h.Distance(mid2) < maxDistance) lane = Lane.Mid;
                if (h.Distance(bot1) < maxDistanceFront || h.Distance(bot2) < maxDistance) lane = Lane.Bot;
                ret.Add(new ChampLane(h, lane));
            }
            return ret;
        }
    }
}

using System;
using System.Linq;
using AutoBuddy.Humanizers;
using AutoBuddy.Utilities;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;

namespace AutoBuddy.MainLogics
{
    internal class Push
    {
        private readonly LogicSelector currentLogic;
        private bool active;
        private Obj_AI_Minion[] currentWave;
        private int CurrentWaveNum;
        private Lane lane;
        private Obj_AI_Base myTurret, enemyTurret;

        private float randomAngle;
        private float randomExtend;
        private Vector3 randomVector;
        private bool wholeWave;

        public Push(LogicSelector current)
        {
            SetRandVector();
            randomVector = new Vector3();
            currentWave = new Obj_AI_Minion[0];
            currentLogic = current;
            Core.DelayAction(SetWaveNumber, 500);
            SetCurrentWave();
            SetOffset();
            if (MainMenu.GetMenu("AB").Get<CheckBox>("debuginfo").CurrentValue)
                Drawing.OnDraw += Drawing_OnDraw;
        }

        private void SetRandVector()
        {
            randomVector.X = RandGen.r.NextFloat(0, 300);
            randomVector.Y = RandGen.r.NextFloat(0, 300);
            Core.DelayAction(SetRandVector, 1000);
        }

        public void Reset(Obj_AI_Base myTower, Obj_AI_Base enemyTower, Lane ln)
        {
            Vector3 pingPos = AutoWalker.p.Distance(AutoWalker.myNexus) - 100 > myTower.Distance(AutoWalker.myNexus)
                ? enemyTower.Position
                : myTower.Position;
            Core.DelayAction(() => SafeFunctions.Ping(PingCategory.OnMyWay, pingPos.Randomized()), RandGen.r.Next(3000));
            lane = ln;
            currentWave = new Obj_AI_Minion[0];
            myTurret = myTower;
            enemyTurret = enemyTower;
            randomExtend = 0;
            currentLogic.SetLogic(LogicSelector.MainLogics.PushLogic);
        }

        public void Activate()
        {
            currentLogic.current = LogicSelector.MainLogics.PushLogic;
            if (active) return;

            Game.OnUpdate += Game_OnUpdate;
            active = true;
        }

        public void Deactivate()
        {
            active = false;
            Game.OnUpdate -= Game_OnUpdate;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (!active) return;
            if (!AutoWalker.p.IsDead && (myTurret.Health <= 0 || enemyTurret.Health <= 0))
            {
                currentLogic.loadLogic.SetLane();
            }
            if (currentWave.Length == 0)
                UnderMyTurret();
            else if (AutoWalker.p.Distance(enemyTurret) < 970+AutoWalker.p.BoundingRadius)
                UnderEnemyTurret();
            else
                Between();
        }


        private void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawText(250, 40, Color.Gold,
                "Push, active: " + active + "  wave num: " + CurrentWaveNum + " minions left: " + currentWave.Length);

            Drawing.DrawCircle(currentWave.Length <= 0 ? AutoWalker.p.Position : AvgPos(currentWave), 100, Color.Aqua);

            if (myTurret != null)
                Drawing.DrawCircle(myTurret.Position, 200, Color.DarkGreen);
            if (enemyTurret != null)
                Drawing.DrawCircle(enemyTurret.Position, 200, Color.DarkRed);
        }

        private void UnderEnemyTurret()
        {
            if (
                ObjectManager.Get<Obj_AI_Minion>()
                    .Count(min => min.IsAlly && min.HealthPercent > 30 && min.Distance(enemyTurret) < 850) < 2)
            {
                Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.None;
                AutoWalker.WalkTo(enemyTurret.Position.Extend(AutoWalker.p, 1100).To3DWorld());
                return;
            }
            if (AutoWalker.p.Distance(enemyTurret) < AutoWalker.p.AttackRange + enemyTurret.BoundingRadius+10 &&
                AutoWalker.p.Distance(enemyTurret) > AutoWalker.p.AttackRange + enemyTurret.BoundingRadius - 10)
            {
                Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.None;
                Player.IssueOrder(GameObjectOrder.AttackUnit, enemyTurret);
            }
            else
            {
                Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.LastHit;
                AutoWalker.WalkTo(enemyTurret.Position.Extend(AutoWalker.p, AutoWalker.p.AttackRange + enemyTurret.BoundingRadius).To3DWorld());
            }
        }

        private void Between()
        {
            Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.LaneClear;
            Vector3 p = AvgPos(currentWave);
            if (p.Distance(AutoWalker.myNexus) > myTurret.Distance(AutoWalker.myNexus))
            {
                AIHeroClient ally =
                    EntityManager.Heroes.Allies.Where(
                        al => !al.IsMe &&
                              AutoWalker.p.Distance(al) < 1200 &&
                              al.Distance(enemyTurret) < p.Distance(enemyTurret) + 150 &&
                              currentLogic.localAwareness.LocalDomination(al) < -15000)
                        .OrderBy(l => l.Distance(AutoWalker.p))
                        .FirstOrDefault();
                if (ally != null)
                    p = ally.Position.Extend(myTurret, 160).To3DWorld() + randomVector;
                if (Math.Abs(p.Distance(AutoWalker.enemyNexus) - AutoWalker.p.Distance(AutoWalker.enemyNexus)) < 200)
                {
                    p =
                        p.Extend(p.Extend(
                            AutoWalker.p.Distance(myTurret) < AutoWalker.p.Distance(enemyTurret)
                                ? myTurret
                                : enemyTurret,
                            400).To3D().RotatedAround(p, 1.57f), randomExtend).To3DWorld();
                }
                AutoWalker.WalkTo(p);
            }
            else
                UnderMyTurret();
        }

        private void UnderMyTurret()
        {
            Vector3 p = new Vector3();
            AIHeroClient ally =
                EntityManager.Heroes.Allies.Where(
                    al => !al.IsMe &&
                          AutoWalker.p.Distance(al) < 1200 && al.Distance(enemyTurret) < p.Distance(enemyTurret) + 150 &&
                          currentLogic.localAwareness.LocalDomination(al) < -15000)
                    .OrderBy(l => l.Distance(AutoWalker.p))
                    .FirstOrDefault();
            if (ally != null)
                p = ally.Position.Extend(myTurret, 160).To3DWorld() + randomVector;
            Orbwalker.ActiveModesFlags = AutoWalker.p.Distance(enemyTurret) < 900
                ? Orbwalker.ActiveModes.LastHit
                : Orbwalker.ActiveModes.LaneClear;
            AutoWalker.WalkTo(ally == null
                ? myTurret.Position.Extend(AutoWalker.p.Position, 350 + randomExtend/2)
                    .To3D()
                    .RotatedAround(myTurret.Position, randomAngle)
                : p);
        }

        public Vector3 AvgPos(Obj_AI_Minion[] objects)
        {
            double x = 0, y = 0;
            foreach (Obj_AI_Minion obj in objects)
            {
                x += obj.Position.X;
                y += obj.Position.Y;
            }
            return new Vector2((float) (x/objects.Count()), (float) (y/objects.Count())).To3DWorld();
        }

        private void SetOffset()
        {
            if (!active)
            {
                Core.DelayAction(SetOffset, 500);
                return;
            }
            float newEx = randomExtend;
            while (Math.Abs(newEx - randomExtend) < 190)
            {
                newEx = RandGen.r.NextFloat(-400, 400);
            }
            randomAngle = RandGen.r.NextFloat(0, 6.28f);
            randomExtend = newEx;
            Core.DelayAction(SetOffset, RandGen.r.Next(800, 1600));
        }

        private void SetWaveNumber()
        {
            Core.DelayAction(SetWaveNumber, 500);
            Obj_AI_Minion closest =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(min => min.IsAlly && min.Name.Length > 13 && min.GetLane() == lane && min.HealthPercent > 80)
                    .OrderBy(min => min.Distance(enemyTurret))
                    .FirstOrDefault();
            if (closest != null)
            {
                CurrentWaveNum = closest.GetWave();
            }
        }

        private void SetCurrentWave()
        {
            if (CurrentWaveNum == 0)
            {
                Core.DelayAction(SetCurrentWave, 1000);
                return;
            }
            currentWave =
                currentWave.Where(
                    min => !min.IsDead)
                    .ToArray();
            if (currentWave.Length > 1)
            {
                Core.DelayAction(SetCurrentWave, 1000);
                return;
            }

            Obj_AI_Minion[] newMinions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(min => min.IsAlly && min.GetLane() == lane && min.GetWave() == CurrentWaveNum)
                    .ToArray();
            if (!wholeWave && newMinions.Length < 7)
            {
                wholeWave = true;

                Core.DelayAction(SetCurrentWave,
                    newMinions.Any(min => min.Distance(AutoWalker.myNexus) < 800) ? 3000 : 300);
            }
            else
            {
                wholeWave = false;
                currentWave = newMinions;
                Core.DelayAction(SetCurrentWave, 1000);
            }
        }
    }
}
using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace Buddy_vs_Bot.MainLogics
{
    internal class Recall
    {
        private GrassObject g;
        private float lastRecallGold;
        private bool active;
        private readonly LogicSelector current;
        private readonly Obj_SpawnPoint spawn;
        private float lastRecallTime;
        public Recall(LogicSelector currentLogic)
        {
            
            lastRecallGold = 30000;
            current = currentLogic;
            foreach (Obj_SpawnPoint so in ObjectManager.Get<Obj_SpawnPoint>().Where(so => so.Team == ObjectManager.Player.Team))
            {
                spawn = so;
            }
            Core.DelayAction(ShouldRecall, 3000);
            if (MainMenu.GetMenu("AB").Get<CheckBox>("debuginfo").CurrentValue)
                Drawing.OnDraw+=Drawing_OnDraw;
        }


        private void ShouldRecall()
        {
            
            if (Math.Abs(lastRecallGold - 30000) < 10) lastRecallGold = AutoWalker.p.Gold;
            if (active)
            {
                Core.DelayAction(ShouldRecall, 500);
                return;
            }
            if (current.current == LogicSelector.MainLogics.CombatLogic)
            {
                Core.DelayAction(ShouldRecall, 500);
                return;
            }

            if (AutoWalker.p.Gold - lastRecallGold > (AutoWalker.p.Level + 1) * 200 || AutoWalker.p.HealthPercent < 25)
            {
                
                current.SetLogic(LogicSelector.MainLogics.RecallLogic);
            }
            Core.DelayAction(ShouldRecall, 500);
        }

        public void Activate()
        {
            if(active) return;
            lastRecallGold = AutoWalker.p.Gold;
            active = true;
            g = null;
            Game.OnUpdate += Game_OnUpdate;
            
        }

        public void Deactivate()
        {
            lastRecallTime = 0;
            active = false;
            Game.OnUpdate -= Game_OnUpdate;
        }
        void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawText(250, 55, System.Drawing.Color.Gold, "Recall, active: " + active + " est.gold: " + (AutoWalker.p.Gold - lastRecallGold));

        }
        private void Game_OnUpdate(EventArgs args)
        {
            Orbwalker.ActiveModesFlags=Orbwalker.ActiveModes.Combo;
            if(ObjectManager.Player.Distance(spawn)<400&&ObjectManager.Player.HealthPercent>85&&ObjectManager.Player.ManaPercent>80)
                current.SetLogic(LogicSelector.MainLogics.PushLogic);
            else if(ObjectManager.Player.Distance(spawn)<1000)
                AutoWalker.WalkTo(spawn.Position);
            else if (!ObjectManager.Player.IsRecalling() && Game.Time > lastRecallTime)
            {
                Obj_AI_Turret nearestTurret =
                    ObjectManager.Get<Obj_AI_Turret>()
                        .Where(t => t.Team == ObjectManager.Player.Team && !t.IsDead)
                        .OrderBy(t => t.Distance(ObjectManager.Player))
                        .First();
                Vector3 recallPos = nearestTurret.Position.Extend(spawn, 300).To3DWorld();
                if (AutoWalker.p.HealthPercent > 35)
                {
                    if (g == null)
                    {
                        g = ObjectManager.Get<GrassObject>()
                            .Where(gr => gr.Distance(AutoWalker.myNexus) < AutoWalker.p.Distance(AutoWalker.myNexus))
                            .OrderBy(gg => gg.Distance(AutoWalker.p)).ElementAt(3);
                    }
                    if (g != null && g.Distance(AutoWalker.p) < nearestTurret.Position.Distance(AutoWalker.p))
                    {
                        
                        Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.None;
                        recallPos = g.Position;
                    }
                }

                if (ObjectManager.Player.Distance(recallPos) < 70)
                {
                    CastRecall();
                }
                else
                    AutoWalker.WalkTo(recallPos);
            }
        }

        private void CastRecall()
        {
            lastRecallGold = AutoWalker.p.Gold;
            if(Game.Time<lastRecallTime) return;
            lastRecallTime = Game.Time + RandGen.r.NextFloat(9f, 11f);
            Core.DelayAction(()=>ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Recall), 400);
        }
    }
}

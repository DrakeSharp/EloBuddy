using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace AutoBuddy.MainLogics
{
    internal class Combat
    {
        private readonly LogicSelector current;
        private bool active;
        private string lastMode = " ";
        private LogicSelector.MainLogics returnTo;

        public Combat(LogicSelector currentLogic)
        {
            current = currentLogic;
            Game.OnUpdate += Game_OnUpdate;
            if (MainMenu.GetMenu("AB").Get<CheckBox>("debuginfo").CurrentValue)
                Drawing.OnDraw += Drawing_OnDraw;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawText(250, 25, System.Drawing.Color.Gold, "Combat, active:  " + active+" last mode: "+lastMode);
        }


        public void Activate()
        {
            if (active) return;
            active = true;
        }

        public void Deactivate()
        {
            active = false;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (current.current == LogicSelector.MainLogics.SurviLogic) return;
            AIHeroClient har = null;
            /*  TODO Remove old logic if worse
             * AIHeroClient victim =
                EntityManager.Heroes.Enemies.Where(
                    vic => vic.Distance(AutoWalker.p) < vic.BoundingRadius + AutoWalker.p.AttackRange + 450 && vic.IsVisible() && vic.Health > 0
                           && AutoWalker.p.HealthPercent/vic.HealthPercent > 1.3 &&
                           (AutoWalker.p.Distance(AutoWalker.p.GetNearestTurret()) > 1100 ||
                            (AutoWalker.p.HealthPercent > 40 && vic.HealthPercent < 10)))
                    .OrderBy(vic => vic.HealthPercent)
                    .FirstOrDefault();
            if (victim != null &&(
                EntityManager.Heroes.Enemies.Count(
                    def =>
                        def.NetworkId != victim.NetworkId &&
                        def.HealthPercent > AutoWalker.p.HealthPercent && def.IsHPBarRendered && def.Distance(victim) < 600 &&
                        def.Distance(AutoWalker.p) < 600))>
                EntityManager.Heroes.Allies.Count(ally => ally.HealthPercent > 5 && ally.IsVisible() && ally.Distance(AutoWalker.p) < 500 && ally.Distance(victim) < 500))
                victim = null;*/
            AIHeroClient victim = null;
            if (current.surviLogic.dangerValue < -15000)
                victim = EntityManager.Heroes.Enemies.Where(
                    vic =>
                        vic.Distance(AutoWalker.p) < vic.BoundingRadius + AutoWalker.p.AttackRange + 450 &&
                        vic.IsVisible() && vic.Health > 0 &&
                        current.localAwareness.MyStrength()/current.localAwareness.HeroStrength(vic) > 1.5)
                    .OrderBy(v => v.Health)
                    .FirstOrDefault();


            if (victim == null || AutoWalker.p.GetNearestTurret().Distance(AutoWalker.p) > 1100)
            {
                har =
                    EntityManager.Heroes.Enemies.Where(
                        h =>
                            h.Distance(AutoWalker.p) < AutoWalker.p.AttackRange + h.BoundingRadius + 50 && h.IsVisible() &&
                            h.HealthPercent > 0).OrderBy(h => h.Distance(AutoWalker.p)).FirstOrDefault();
            }


            if ((victim != null || har != null) && !active)
            {
                LogicSelector.MainLogics returnT = current.SetLogic(LogicSelector.MainLogics.CombatLogic);
                if (returnT != LogicSelector.MainLogics.SurviLogic) returnTo = returnT;
            }

            if (!active)
                return;
            if (victim == null && har == null)
            {
                current.SetLogic(returnTo);
                return;
            }

            if (victim != null)
            {
                Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.Combo;
                current.myChamp.Combo(victim);
                Vector3 vicPos=Prediction.Position.PredictUnitPosition(victim, 500).To3D();
                Vector3 posToWalk =
                    vicPos.Extend(AutoWalker.myNexus,
                        (victim.BoundingRadius + AutoWalker.p.AttackRange - 30)*
                        Math.Min(current.localAwareness.HeroStrength(victim)/current.localAwareness.MyStrength()*2f, 1))
                        .To3DWorld();

                Obj_AI_Turret nearestEnemyTurret = posToWalk.GetNearestTurret();
                lastMode = "combo";
                if (
                    AutoWalker.p.Distance(nearestEnemyTurret)<950+AutoWalker.p.BoundingRadius)
                {
                    if (victim.Health > AutoWalker.p.GetAutoAttackDamage(victim) + 15||victim.Distance(AutoWalker.p)>AutoWalker.p.AttackRange+victim.BoundingRadius-20)
                    {
                        lastMode = "enemy under turret, ignoring";
                        current.SetLogic(returnTo);
                        return;
                    }
                    lastMode = "combo under turret";
                }

                AutoWalker.WalkTo(posToWalk);


                if (AutoWalker.Ghost != null && AutoWalker.Ghost.IsReady() &&
                    AutoWalker.p.HealthPercent/victim.HealthPercent > 2 &&
                    victim.Distance(AutoWalker.p) > AutoWalker.p.AttackRange + victim.BoundingRadius + 100 &&
                    victim.Distance(victim.Position.GetNearestTurret()) > 1500)
                    AutoWalker.Ghost.Cast();
            }
            else
            {
                Vector3 harPos = Prediction.Position.PredictUnitPosition(har, 500).To3D();
                harPos=harPos.Extend(AutoWalker.p.Position, AutoWalker.p.AttackRange + har.BoundingRadius-20).To3D();
                lastMode = "harass";
                Obj_AI_Turret tu = harPos.GetNearestTurret();
                
                if (harPos.Distance(tu) < 1090)
                {

                    harPos = tu.Position.Extend(harPos, 1090).To3DWorld();
                    lastMode = "harass under turret";

                    if (harPos.Distance(AutoWalker.myNexus) > tu.Distance(AutoWalker.myNexus))
                        harPos =
                            tu.Position.Extend(AutoWalker.myNexus, 1050 + AutoWalker.p.BoundingRadius).To3DWorld();
                    
                }

                Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.Harass;
                current.myChamp.Harass(har);
                

                AutoWalker.WalkTo(harPos);
            }
        }
    }
}
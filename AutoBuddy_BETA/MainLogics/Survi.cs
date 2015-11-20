using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;
using Color = System.Drawing.Color;

namespace Buddy_vs_Bot.MainLogics
{
    internal class Survi
    {
        private LogicSelector.MainLogics returnTo;
        private float spierdalanko;
        private bool active;
        private readonly LogicSelector current;
        private int hits;
        public float dangerValue;
        public Survi(LogicSelector currentLogic)
        {
            current = currentLogic;
            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnSpellCast += Obj_AI_Base_OnSpellCast;
            DecHits();
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private void Obj_AI_Base_OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender == null || args.Target == null) return;
            if (!args.Target.IsMe) return;
            if (sender.IsAlly) return;
            if(sender.Type==GameObjectType.obj_AI_Turret) SetSpierdalanko((1100-AutoWalker.p.Distance(sender))/AutoWalker.p.MoveSpeed);
            else if (sender.Type == GameObjectType.obj_AI_Minion) hits++;
            else if (sender.Type == GameObjectType.AIHeroClient) hits+=2;
        }


        private void SetSpierdalanko(float sec)
        {
            spierdalanko = Game.Time + sec;
            if (active || (current.current == LogicSelector.MainLogics.CombatLogic&&AutoWalker.p.HealthPercent>13)) return;
            LogicSelector.MainLogics returnT = current.SetLogic(LogicSelector.MainLogics.SurviLogic);
            if (returnT != LogicSelector.MainLogics.SurviLogic) returnTo = returnT;
        }
        private void SetSpierdalankoUnc(float sec)
        {
            spierdalanko = Game.Time + sec;
            if (active) return;
            LogicSelector.MainLogics returnT = current.SetLogic(LogicSelector.MainLogics.SurviLogic);
            if (returnT != LogicSelector.MainLogics.SurviLogic) returnTo = returnT;
        }

        public void Activate()
        {
            if(active) return;
            active = true;

            
        }

        void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawText(250, 10, Color.Gold, "Survi, active: " + active + "  hits: " + hits + "  dangervalue: " + dangerValue);
        }

        public void Deactivate()
        {
            active = false;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if(hits*20>AutoWalker.p.HealthPercent) SetSpierdalanko(.5f);
            dangerValue = current.localAwareness.LocalDomination(AutoWalker.p);
            if (dangerValue > -2000)
            {
                SetSpierdalankoUnc(2);
                current.saveMylife = true;
            }
            if (!active)
                return;
            if (Game.Time > spierdalanko)
            {
                current.saveMylife = false;
                current.SetLogic(returnTo);
            }
            Vector3 closestSafePoint =
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(tur => tur.IsAlly && tur.Health > 0)
                    .OrderBy(tur => tur.Distance(AutoWalker.p))
                    .First().Position;
            if (closestSafePoint.Distance(AutoWalker.p) > 2000)
            {
                AIHeroClient ally=EntityManager.Heroes.Allies.Where(
                    a => a.Distance(AutoWalker.p) < 2000 && current.localAwareness.LocalDomination(a.Position) < -40000)
                    .OrderBy(al => al.Distance(AutoWalker.p))
                    .FirstOrDefault();
                if (ally != null)
                    closestSafePoint = ally.Position;
            }
            Orbwalker.ActiveModesFlags=AutoWalker.p.Distance(closestSafePoint)<400?Orbwalker.ActiveModes.Combo:Orbwalker.ActiveModes.None;
            AutoWalker.WalkTo(closestSafePoint.Extend(AutoWalker.myNexus, 200).To3DWorld());
            if (AutoWalker.p.HealthPercent < 10||AutoWalker.p.HealthPercent < 20 && AutoWalker.Heal != null && AutoWalker.Heal.IsReady()&&EntityManager.Heroes.Enemies.Any(en=>en.IsVisible()&&en.Distance(AutoWalker.p)<600))
                AutoWalker.Heal.Cast();
            if (AutoWalker.Ghost.IsReady()&&dangerValue>20000)
                AutoWalker.Ghost.Cast();
            if (ObjectManager.Player.HealthPercent < 35)
            {
                var hppot = new Item(ItemId.Health_Potion);
                if (hppot.IsOwned())
                    hppot.Cast();

            }
            current.myChamp.Survi();
        }

        private void DecHits()
        {
            if (hits > 4)
                hits = 4;
            if (hits > 0)
                hits--;
            Core.DelayAction(DecHits, 800);
        }

    }
}

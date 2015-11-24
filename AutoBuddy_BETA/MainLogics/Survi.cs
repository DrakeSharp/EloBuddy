using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;

namespace AutoBuddy.MainLogics
{
    internal class Survi
    {
        private readonly LogicSelector current;
        private bool active;
        public float dangerValue;
        private int hits;
        private LogicSelector.MainLogics returnTo;
        private float spierdalanko;

        public Survi(LogicSelector currentLogic)
        {
            current = currentLogic;
            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnSpellCast += Obj_AI_Base_OnSpellCast;
            DecHits();
            if (MainMenu.GetMenu("AB").Get<CheckBox>("debuginfo").CurrentValue)
                Drawing.OnDraw += Drawing_OnDraw;
        }

        private void Obj_AI_Base_OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender == null || args.Target == null) return;
            if (!args.Target.IsMe) return;
            if (sender.IsAlly) return;
            if (sender.Type == GameObjectType.obj_AI_Turret)
                SetSpierdalanko((1100 - AutoWalker.p.Distance(sender))/AutoWalker.p.MoveSpeed);
            else if (sender.Type == GameObjectType.obj_AI_Minion) hits++;
            else if (sender.Type == GameObjectType.AIHeroClient) hits += 2;
        }


        private void SetSpierdalanko(float sec)
        {
            spierdalanko = Game.Time + sec;
            if (active || (current.current == LogicSelector.MainLogics.CombatLogic && AutoWalker.p.HealthPercent() > 13))
                return;
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
            if (active) return;
            active = true;
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawText(250, 10, Color.Gold,
                "Survi, active: " + active + "  hits: " + hits + "  dangervalue: " + dangerValue);
        }

        public void Deactivate()
        {
            active = false;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (hits*20 > AutoWalker.p.HealthPercent())
            {
                SetSpierdalanko(.5f);
            }
            dangerValue = current.localAwareness.LocalDomination(AutoWalker.p);
            if (dangerValue > -2000||AutoWalker.p.Distance(AutoWalker.enemyLazer)<1600)
            {
                
                SetSpierdalankoUnc(1.8f);
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
                AIHeroClient ally = EntityManager.Heroes.Allies.Where(
                    a => a.Distance(AutoWalker.p) < 1500 && current.localAwareness.LocalDomination(a.Position) < -40000)
                    .OrderBy(al => al.Distance(AutoWalker.p))
                    .FirstOrDefault();
                if (ally != null)
                    closestSafePoint = ally.Position;
            }
            if (closestSafePoint.Distance(AutoWalker.p) > 150)
            {
                AIHeroClient ene =
                    EntityManager.Heroes.Enemies
                        .FirstOrDefault(en => en.Health > 0 && en.Distance(closestSafePoint) < 300);
                if (ene != null)
                {
                    closestSafePoint = AutoWalker.myNexus.Position;
                }
            }

            Orbwalker.ActiveModesFlags = AutoWalker.p.Distance(closestSafePoint) < 200
                ? Orbwalker.ActiveModes.Combo
                : Orbwalker.ActiveModes.None;
            AutoWalker.WalkTo(closestSafePoint.Extend(AutoWalker.myNexus, 200).To3DWorld());
            if (AutoWalker.p.HealthPercent() < 10 ||
                AutoWalker.p.HealthPercent() < 20 && AutoWalker.Heal != null && AutoWalker.Heal.IsReady() &&
                EntityManager.Heroes.Enemies.Any(en => en.IsVisible() && en.Distance(AutoWalker.p) < 600))
                AutoWalker.Heal.Cast();
            if (AutoWalker.Ghost.IsReady() && dangerValue > 20000)
                AutoWalker.Ghost.Cast();
            if (ObjectManager.Player.HealthPercent() < 35)
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
            Core.DelayAction(DecHits, 700);
        }
    }
}
using System;
using System.Linq;
using AutoBuddy.Utilities;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;

namespace AutoBuddy
{
    internal static class AutoWalker
    {
        public static readonly Spell.Active Heal;
        public static readonly Spell.Active Ghost;
        public static readonly Obj_HQ MyNexus;
        public static readonly Obj_HQ EneMyNexus;
        public static readonly AIHeroClient p;
        public static readonly Obj_AI_Turret EnemyLazer;
        private static Orbwalker.ActiveModes _activeMode = Orbwalker.ActiveModes.None;

        static AutoWalker()
        {
            MyNexus = ObjectManager.Get<Obj_HQ>().First(n => n.IsAlly);
            EneMyNexus = ObjectManager.Get<Obj_HQ>().First(n => n.IsEnemy);
            EnemyLazer =
                ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(tur => !tur.IsAlly && tur.GetLane() == Lane.Spawn);
            p = ObjectManager.Player;

            if (p.Spellbook.GetSpell(SpellSlot.Summoner1).Name == "summonerheal")
            {
                Heal = new Spell.Active(SpellSlot.Summoner1);
            }
            if (p.Spellbook.GetSpell(SpellSlot.Summoner2).Name == "summonerheal")
            {
                Heal = new Spell.Active(SpellSlot.Summoner2);
            }
            if (p.Spellbook.GetSpell(SpellSlot.Summoner1).Name == "summonerhaste")
            {
                Ghost = new Spell.Active(SpellSlot.Summoner1);
            }
            if (p.Spellbook.GetSpell(SpellSlot.Summoner2).Name == "summonerhaste")
            {
                Ghost = new Spell.Active(SpellSlot.Summoner2);
            }

            Target = ObjectManager.Player.Position;
            Orbwalker.DisableMovement = false;

            Orbwalker.DisableAttacking = false;
            Game.OnUpdate += Game_OnUpdate;

            if (Orbwalker.HoldRadius > 130 || Orbwalker.HoldRadius < 80)
            {
                Chat.Print("=================WARNING=================", Color.Red);
                Chat.Print("Your hold radius value in orbwalker isn't optimal for AutoBuddy", Color.Aqua);
                Chat.Print("Please set hold radius through menu=>Orbwalker");
                Chat.Print("Recommended values: Hold radius: 80-130, Delay between movements: 100-250");
            }

            //PropertyInfo property2 = typeof(Orbwalker).GetProperty("HoldRadius");

            //property2.GetSetMethod(true).Invoke(null, new object[] { 20 });
            if (MainMenu.GetMenu("AB").Get<CheckBox>("debuginfo").CurrentValue)
                Drawing.OnDraw += Drawing_OnDraw;
        }


        public static Vector3 Target { get; private set; }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (_activeMode == Orbwalker.ActiveModes.LaneClear)
            {
                Orbwalker.ActiveModesFlags =
                    EntityManager.MinionsAndMonsters.EnemyMinions.Any(
                        en =>
                            en.Distance(p) < p.AttackRange + en.BoundingRadius &&
                            Prediction.Health.GetPrediction(en, 2000) < p.GetAutoAttackDamage(en))
                        ? Orbwalker.ActiveModes.LastHit
                        : Orbwalker.ActiveModes.LaneClear;
            }
            else
                Orbwalker.ActiveModesFlags = _activeMode;
        }

        public static void SetMode(Orbwalker.ActiveModes mode)
        {
            _activeMode = mode;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawCircle(Target, 30, Color.BlueViolet);
        }

        public static void WalkTo(Vector3 tgt)
        {
            Target = tgt;
            Orbwalker.OverrideOrbwalkPosition = () => Target;
        }
    }
}
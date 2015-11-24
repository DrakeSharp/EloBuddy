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

namespace AutoBuddy
{
    internal static class AutoWalker
    {
        public static Spell.Active Heal;
        public static Spell.Active Ghost;
        public static Obj_HQ myNexus;
        public static Obj_HQ enemyNexus;
        public static AIHeroClient p;
        public static int maxAdditionalTime = 50;
        public static int adjustAnimation = 20;
        public static float holdRadius = 100;
        public static float movementDelay = .25f;
        public static Obj_AI_Turret enemyLazer;
        private static Orbwalker.ActiveModes activeMode=Orbwalker.ActiveModes.None;

        private static float nextMove;

        static AutoWalker()
        {
            myNexus = ObjectManager.Get<Obj_HQ>().First(n => n.IsAlly);
            enemyNexus = ObjectManager.Get<Obj_HQ>().First(n => n.IsEnemy);
            enemyLazer = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(tur => !tur.IsAlly && tur.GetLane() == Lane.Spawn);
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

            target = ObjectManager.Player.Position;
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
            if (MainMenu.GetMenu("AB").Get<CheckBox>("debuginfo").CurrentValue)
                Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (activeMode == Orbwalker.ActiveModes.LaneClear)
            {
                

                    if (EntityManager.MinionsAndMonsters.EnemyMinions.Any(
                        en => en.Distance(p) < p.AttackRange + en.BoundingRadius&&Prediction.Health.GetPrediction(en, 2000)<p.GetAutoAttackDamage(en)))
                {
                    Orbwalker.ActiveModesFlags=Orbwalker.ActiveModes.LastHit;
                }else
                {
                    Orbwalker.ActiveModesFlags=Orbwalker.ActiveModes.LaneClear;
                }
            }
            else
                Orbwalker.ActiveModesFlags = activeMode;
        }


        public static Vector3 target { get; private set; }

        public static void SetMode(Orbwalker.ActiveModes mode)
        {
            activeMode = mode;
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawCircle(target, 30, Color.BlueViolet);
        }

        public static void WalkTo(Vector3 tgt)
        {
            
            target = tgt;
            Orbwalker.OverrideOrbwalkPosition = () => target;
        }

    }
}
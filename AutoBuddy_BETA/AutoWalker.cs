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

        private static float nextMove;

        static AutoWalker()
        {
            myNexus = ObjectManager.Get<Obj_HQ>().First(n => n.IsAlly);
            enemyNexus = ObjectManager.Get<Obj_HQ>().First(n => n.IsEnemy);
            enemyLazer = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(tur => tur.IsEnemy && tur.GetLane() == Lane.Spawn);
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
            Orbwalker.DisableMovement = true;
            Orbwalker.OnPreAttack += Orbwalker_OnPreAttack;
            Game.OnTick += OnTick;
            if (MainMenu.GetMenu("AB").Get<CheckBox>("debuginfo").CurrentValue)
                Drawing.OnDraw += Drawing_OnDraw;
        }

        public static Vector3 target { get; private set; }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawCircle(target, 30, Color.BlueViolet);
        }

        public static void WalkTo(Vector3 tgt)
        {
            target = tgt;
        }

        private static void Orbwalker_OnPreAttack(AttackableUnit tgt, Orbwalker.PreAttackArgs args)
        {
            nextMove = Game.Time + ObjectManager.Player.AttackCastDelay +
                       (Game.Ping + adjustAnimation + RandGen.r.Next(maxAdditionalTime))/1000f;
        }

        private static void OnTick(EventArgs args)
        {
            if (ObjectManager.Player.Position.Distance(target) < 50 || Game.Time < nextMove) return;
            nextMove = Game.Time + movementDelay;
            Player.IssueOrder(GameObjectOrder.MoveTo, target, true);
        }
    }
}
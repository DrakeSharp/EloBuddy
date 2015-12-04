using System;
using System.Linq;
using AutoBuddy.Humanizers;
using AutoBuddy.Utilities;
using AutoBuddy.Utilities.AutoShop;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace AutoBuddy
{
    internal static class AutoWalker
    {
        public static Spell.Active Ghost, Barrier;
        public static Spell.Skillshot Flash;
        public static Spell.Targeted Heal, Teleport, Ignite, Smite, Exhaust;
        public static readonly Obj_HQ MyNexus;
        public static readonly Obj_HQ EneMyNexus;
        public static readonly AIHeroClient p;
        public static readonly Obj_AI_Turret EnemyLazer;
        private static Orbwalker.ActiveModes activeMode = Orbwalker.ActiveModes.None;
        private static InventorySlot seraphs;
        private static readonly ColorBGRA color;


        static AutoWalker()
        {
            color = new ColorBGRA(79, 219, 50, 255);
            MyNexus = ObjectManager.Get<Obj_HQ>().First(n => n.IsAlly);
            EneMyNexus = ObjectManager.Get<Obj_HQ>().First(n => n.IsEnemy);
            EnemyLazer =
                ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(tur => !tur.IsAlly && tur.GetLane() == Lane.Spawn);
            p = ObjectManager.Player;
            initSummonerSpells();

            Target = ObjectManager.Player.Position;
            Orbwalker.DisableMovement = false;

            Orbwalker.DisableAttacking = false;
            Game.OnUpdate += Game_OnUpdate;
            Orbwalker.OverrideOrbwalkPosition = () => Target;
            if (Orbwalker.HoldRadius > 130 || Orbwalker.HoldRadius < 80)
            {
                Chat.Print("=================WARNING=================", Color.Red);
                Chat.Print("Your hold radius value in orbwalker isn't optimal for AutoBuddy", Color.Aqua);
                Chat.Print("Please set hold radius through menu=>Orbwalker");
                Chat.Print("Recommended values: Hold radius: 80-130, Delay between movements: 100-250");
            }
            if (MainMenu.GetMenu("AB").Get<CheckBox>("debuginfo").CurrentValue)
                Drawing.OnDraw += Drawing_OnDraw;
            updateItems();
            oldOrbwalk();
        }


        public static Vector3 Target { get; private set; }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (activeMode == Orbwalker.ActiveModes.LaneClear)
            {
                Orbwalker.ActiveModesFlags = (p.TotalAttackDamage < 150 &&
                    EntityManager.MinionsAndMonsters.EnemyMinions.Any(
                        en =>
                            en.Distance(p) < p.AttackRange + en.BoundingRadius &&
                            Prediction.Health.GetPrediction(en, 2000) < p.GetAutoAttackDamage(en))
                        ) ? Orbwalker.ActiveModes.Harass
                        : Orbwalker.ActiveModes.LaneClear;
            }
            else
                Orbwalker.ActiveModesFlags = activeMode;
        }

        public static void SetMode(Orbwalker.ActiveModes mode)
        {
            if (activeMode != Orbwalker.ActiveModes.Combo)
                Orbwalker.DisableAttacking = false;
            activeMode = mode;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Circle.Draw(color,40, Target );
        }

        public static void WalkTo(Vector3 tgt)
        {
            Target = tgt;
        }

        private static void updateItems()
        {
            seraphs = p.InventoryItems.FirstOrDefault(it => (int)it.Id == 3040);
            Core.DelayAction(updateItems, 5000);
        }
        public static void UseSeraphs()
        {
            if (seraphs != null && seraphs.CanUseItem())
                seraphs.Cast();
        }
        public static void UseGhost()
        {
            if (Ghost != null && Ghost.IsReady())
                Ghost.Cast();
        }
        public static void UseBarrier()
        {
            if (Barrier != null && Barrier.IsReady())
                Barrier.Cast();
        }
        public static void UseHeal()
        {
            if (Heal != null && Heal.IsReady())
                Heal.Cast();
        }
        public static void UseIgnite(AIHeroClient target = null)
        {
            if (Ignite == null || !Ignite.IsReady()) return;
            if (target == null)target =
                    EntityManager.Heroes.Enemies.Where(en => en.Distance(p) < 600)
                        .OrderBy(en => en.Health)
                        .FirstOrDefault();
            if (target != null && p.Distance(target) < 600 + target.BoundingRadius)
            {
                Ignite.Cast(target);
            }
                
        }

        private static void initSummonerSpells()
        {
            Barrier = Player.Spells.FirstOrDefault(sp => sp.SData.Name.Contains("summonerbarrier")) == null ? null : new Spell.Active(ObjectManager.Player.GetSpellSlotFromName("summonerbarrier"));
            Ghost = Player.Spells.FirstOrDefault(sp => sp.SData.Name.Contains("summonerhaste")) == null ? null : new Spell.Active(ObjectManager.Player.GetSpellSlotFromName("summonerhaste"));

            Flash = Player.Spells.FirstOrDefault(sp => sp.SData.Name.Contains("summonerflash")) == null ? null : new Spell.Skillshot(ObjectManager.Player.GetSpellSlotFromName("summonerflash"), 600, SkillShotType.Circular);

            Heal = Player.Spells.FirstOrDefault(sp => sp.SData.Name.Contains("summonerheal")) == null ? null : new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerheal"), 600);
            Ignite = Player.Spells.FirstOrDefault(sp => sp.SData.Name.Contains("summonerdot")) == null ? null : new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Exhaust = Player.Spells.FirstOrDefault(sp => sp.SData.Name.Contains("summonerexhaust")) == null ? null : new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerexhaust"), 600);
            Teleport = Player.Spells.FirstOrDefault(sp => sp.SData.Name.Contains("summonerteleport")) == null ? null : new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerteleport"), int.MaxValue);
            Smite = Player.Spells.FirstOrDefault(sp => sp.SData.Name.Contains("smite")) == null ? null : new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("smite"), 600);


        }



#region old orbwalking, for those with not working orbwalker

        private static int maxAdditionalTime = 50;
        private static int adjustAnimation = 20;
        public static float holdRadius = 50;
        private static float movementDelay = .25f;

        private static float nextMove;



        private static void oldOrbwalk()
        {
            if (!MainMenu.GetMenu("AB").Get<CheckBox>("oldWalk").CurrentValue) return;
            Game.OnTick += OnTick;
            Orbwalker.OnPreAttack+=Orbwalker_OnPreAttack;
        }



        private static void Orbwalker_OnPreAttack(AttackableUnit tgt, Orbwalker.PreAttackArgs args)
        {
            nextMove = Game.Time + ObjectManager.Player.AttackCastDelay +
                       (Game.Ping + adjustAnimation + RandGen.r.Next(maxAdditionalTime)) / 1000f;
        }

        private static void OnTick(EventArgs args)
        {
            if (ObjectManager.Player.Position.Distance(Target) < holdRadius || Game.Time < nextMove) return;
            nextMove = Game.Time + movementDelay;
            Player.IssueOrder(GameObjectOrder.MoveTo, Target, true);
        }
    }

#endregion





}
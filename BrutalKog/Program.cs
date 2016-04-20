using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace BrutalKog
{
    internal static class Program
    {
        private static Spell.Skillshot Q, E, R;
        public static Spell.Active W { get; set; }
        private static Menu menu;
        private const string ChampionName = "KogMaw";
        private static AIHeroClient champ;

        public static void Main()
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.BaseSkinName != ChampionName) return;

            menu = MainMenu.AddMenu("Brutal Kog", "kog");

            menu.Add("combo", new KeyBind("Combo", false, KeyBind.BindTypes.HoldActive, 32));

            Q = new Spell.Skillshot(SpellSlot.Q, 1175, SkillShotType.Linear, 250, 1650, 70)
            {
                AllowedCollisionCount = 0
            };
            E = new Spell.Skillshot(SpellSlot.E, 1280, SkillShotType.Linear, 250, 1400, 120)
            {
                AllowedCollisionCount = int.MaxValue
            };
            Obj_AI_Base.OnLevelUp += Obj_AI_Base_OnLevelUp;
            ConfigSpells();

            Game.OnTick += Game_OnTick;
        }

        static void Game_OnTick(EventArgs args)
        {
            AIHeroClient target = TargetSelector.GetTarget(R.Range, DamageType.Magical);

            PredictionResult rese = Prediction.Position.PredictCircularMissile(target, R.Range, R.Radius, R.CastDelay, R.Speed);

            if (rese.HitChancePercent > 50)
                E.Cast(rese.CastPosition);
            if (R.IsLearned && R.IsReady())
            {
                PredictionResult resr = Prediction.Position.PredictCircularMissile(target, R.Range, R.Radius,
                    R.CastDelay, R.Speed);

                if (resr.HitChancePercent > 60)
                    R.Cast(resr.CastPosition);
            }
        }

        static void Obj_AI_Base_OnLevelUp(Obj_AI_Base sender, Obj_AI_BaseLevelUpEventArgs args)
        {
            if (sender == champ) ConfigSpells();
        }

        private static void ConfigSpells()
        {
            W = new Spell.Active(SpellSlot.W, (uint)champ.GetAutoAttackRange());
            R = new Spell.Skillshot(SpellSlot.R,
            (uint)(900 + 300 * champ.Spellbook.GetSpell(SpellSlot.R).Level),
            SkillShotType.Circular, 1200, int.MaxValue, 225)
            {
                AllowedCollisionCount = int.MaxValue
            };
        }
    }
}

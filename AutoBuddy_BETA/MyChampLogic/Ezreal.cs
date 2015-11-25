using System.Collections.Generic;
using System.Linq;
using AutoBuddy.MainLogics;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;

namespace AutoBuddy.MyChampLogic
{
    internal class Ezreal : IChampLogic
    {
        /* Made By: MarioGK */
        public int[] skillSequence { get; private set; }
        public LogicSelector Logic { get; set; }
        public string ShopSequence { get; private set; }

        private readonly Spell.Skillshot Q;
        private readonly Spell.Skillshot W;
        private readonly Spell.Skillshot E;
        private readonly Spell.Skillshot R;

        public Ezreal()
        {
            //                     1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16 17 18
            skillSequence = new[] {1, 3, 1, 2, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2};
            ShopSequence =
                "1055:1, 3340:1, 3070:1, 1036:1, 2003:1, 3042:1, 3044:1, 3057:1, 3086:1, 3078:1, 3158:1, 1037:1, 3035:1, 3144:1, 1043:1, 3153:1 , 1036:1, 1038:1 , 1053:1, 3072:1";

            Q = new Spell.Skillshot(SpellSlot.Q, 1160, SkillShotType.Linear, 350, 2000, 65)
            {
                MinimumHitChance = HitChance.High
            };
            W = new Spell.Skillshot(SpellSlot.W, 970, SkillShotType.Linear, 350, 1550, 80)
            {
                AllowedCollisionCount = int.MaxValue
            };
            E = new Spell.Skillshot(SpellSlot.E, 470, SkillShotType.Circular, 450, int.MaxValue, 10);
            R = new Spell.Skillshot(SpellSlot.R, 2000, SkillShotType.Linear, 1, 2000, 160)
            {
                MinimumHitChance = HitChance.High,
                AllowedCollisionCount = int.MaxValue
            };

            Game.OnTick += Game_OnTick;
        }

        public void Harass(AIHeroClient target)
        {
            if (Q.IsReady() && target.IsValidTarget(Q.Range) && 20 <= Player.Instance.ManaPercent)
            {
                Q.Cast(target);
            }

            if (W.IsReady() && target.IsValidTarget(W.Range) && 30 <= Player.Instance.ManaPercent)
            {
                W.Cast(target);
            }
        }

        public void Survi()
        {
            if (E.IsReady() && Player.Instance.CountEnemiesInRange(800) <= 1)
            {
                E.Cast(Player.Instance.Position.Extend(AutoWalker.Target, E.Range).To3D());
            }
        }

        public void Combo(AIHeroClient target)
        {
            if (E.IsReady() && Player.Instance.CountEnemiesInRange(800) <= 2 && Player.Instance.ManaPercent >= 60 || target.HealthPercent <= 20)
            {
                E.Cast(Player.Instance.Position.Extend(target.Position, E.Range).To3D());
            }

            if (Q.IsReady() && target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }

            if (W.IsReady() && target.IsValidTarget(W.Range) && Player.Instance.ManaPercent > 40)
            {
                W.Cast(target);
            }

            if (R.IsReady() && Player.Instance.CountEnemiesInRange(W.Range) <= 1)
            {
                var heroes = EntityManager.Heroes.Enemies;
                foreach (var hero in EntityManager.Heroes.Enemies.Where(hero => !hero.IsDead && hero.IsVisible && hero.IsInRange(Player.Instance, R.Range)))
                {
                    var collision = new List<AIHeroClient>();
                    collision.Clear();
                    var hero1 = hero;
                    foreach (var colliHero in heroes.Where(colliHero => !colliHero.IsDead && colliHero.IsVisible && colliHero.IsInRange(hero1, 3000)))
                    {
                        if (Prediction.Position.Collision.LinearMissileCollision(colliHero, Player.Instance.Position.To2D(), Player.Instance.Position.Extend(hero.Position.To2D(), 1500),
                            R.Speed, R.Width, R.CastDelay))
                        {
                            collision.Add(colliHero);
                        }
                        if (collision.Count >= 3)
                        {
                            R.Cast(hero);
                        }
                    }
                }
            }
        }

        void Game_OnTick(System.EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                var laneMinion =
                    EntityManager.MinionsAndMonsters.GetLaneMinions()
                        .OrderByDescending(m => m.Health)
                        .FirstOrDefault(
                            m => m.IsValidTarget(Q.Range) && m.Health <= Player.Instance.GetSpellDamage(m, SpellSlot.Q));
                if (laneMinion == null) return;

                if (Q.IsReady() && 40 <= Player.Instance.ManaPercent)
                {
                    Q.Cast(laneMinion);
                }
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                {
                    var lastMinion =
                        EntityManager.MinionsAndMonsters.GetLaneMinions()
                            .OrderByDescending(m => m.Health)
                            .FirstOrDefault(
                                m => m.IsValidTarget(Q.Range) && !m.IsInRange(Player.Instance, Player.Instance.GetAutoAttackRange()) && m.Health <= Player.Instance.GetSpellDamage(m, SpellSlot.Q));
                    if (lastMinion == null) return;

                    if (Q.IsReady() && 20 <= Player.Instance.ManaPercent)
                    {
                        Q.Cast(lastMinion);
                    }
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutoBuddy.MainLogics;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;

namespace AutoBuddy.MyChampLogic
{
    internal class Cassiopeia : IChampLogic
    {
        private Spell.Skillshot Q, W, R;
        private Spell.Targeted E;
        private int minManaHarass = 35;
        private float hitchance = 0;
        public Cassiopeia()
        {
            ShopSequence =
                "3340:Buy,2003:StartHpPot,1056:Buy,1027:Buy,3070:Buy,1001:Buy,1058:Buy,3003:Buy,3020:Buy,1028:Buy,1011:Buy,1058:Buy,2003:StopHpPot,3116:Buy,1004:Buy,1004:Buy,3114:Buy,1052:Buy,3108:Buy,3165:Buy,1056:Sell,1058:Buy,3089:Buy,1028:Buy,3136:Buy,3151:Buy";
            Q=new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Circular, 750, int.MaxValue, 40);
            W = new Spell.Skillshot(SpellSlot.W, 850, SkillShotType.Circular, 600, 2500, 90);
            R=new Spell.Skillshot(SpellSlot.R, 500, SkillShotType.Cone, 650, int.MaxValue, 75);
            E=new Spell.Targeted(SpellSlot.E, 700);
            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawText(900, 10, Color.Chocolate, "Last hitchance:"+hitchance, 10);
        }

        private void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Harass || Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LaneClear)
            {
                if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LaneClear && AutoWalker.p.ManaPercent < 80 && !EntityManager.Heroes.Enemies.Any(en => en.Distance(AutoWalker.p) < 850))
                {
                    
                }


                if(AutoWalker.p.ManaPercent<35)return;
                AIHeroClient poorVictim = TargetSelector.GetTarget(850, DamageType.Magical, addBoundingRadius: true);
                if (poorVictim != null && minManaHarass < AutoWalker.p.HealthPercent)
                {
                    if (Q.IsReady())
                    {
                        PredictionResult pr = Q.GetPrediction(poorVictim);
                        hitchance = pr.HitChancePercent;
                        float x = AutoWalker.p.HealthPercent;
                        if (pr.HitChancePercent >
                            -0.00084981684 * x*x*x + 0.1654212454 * x*x - 10.74139194 * x + 309.7435897)
                            Q.Cast(pr.CastPosition);
                    }
                    if (E.IsReady())
                    {
                        AIHeroClient candidateForE = EntityManager.Heroes.Enemies.Where(
                            en =>
                                en.HasBuffOfType(BuffType.Poison) && en.IsTargetable &&
                                !en.HasBuffOfType(BuffType.SpellImmunity) && !en.HasBuffOfType(BuffType.Invulnerability) &&
                                en.Distance(AutoWalker.p) < en.BoundingRadius + E.Range && !en.IsDead())
                            .OrderBy(en => en.Health/AutoWalker.p.GetSpellDamage(en, SpellSlot.E))
                            .FirstOrDefault();
                        if (candidateForE != null)
                            E.Cast(candidateForE);
                    }
                }
            }
            else if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
            {
                AIHeroClient poorVictim = TargetSelector.GetTarget(700, DamageType.Magical, addBoundingRadius: true) ??
                                          TargetSelector.GetTarget(850, DamageType.Magical, addBoundingRadius: true);

                if (poorVictim != null)
                {
                    if (Q.IsReady())
                    {
                        PredictionResult pr = Q.GetPrediction(poorVictim);
                        hitchance = pr.HitChancePercent;
                        if (pr.HitChancePercent >
                            -0.003 * AutoWalker.p.HealthPercent * AutoWalker.p.HealthPercent + 0.65 * AutoWalker.p.HealthPercent + 35)
                            Q.Cast(pr.CastPosition);
                    }
                    if (E.IsReady() && (poorVictim.HasBuffOfType(BuffType.Poison) || AutoWalker.p.GetSpellDamage(poorVictim, SpellSlot.E)>poorVictim.Health))
                        E.Cast(poorVictim);
                    
                }
            }
        }

        public int[] skillSequence { get; private set; }
        public LogicSelector Logic { get; set; }
        

        public string ShopSequence { get; private set; }

        public void Harass(AIHeroClient target)
        {
        }

        public void Survi()
        {
        }

        public void Combo(AIHeroClient target)
        {

        }
    }
}
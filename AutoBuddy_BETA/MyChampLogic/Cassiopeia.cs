using System;
using System.Linq;
using AutoBuddy.Humanizers;
using AutoBuddy.MainLogics;
using AutoBuddy.Utilities.AutoShop;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;

namespace AutoBuddy.MyChampLogic
{
    internal class Cassiopeia : IChampLogic
    {
        public float MaxDistanceForAA { get{ return 500; } }
        public float OptimalMaxComboDistance { get { return 500; } }
        public float HarassDistance { get { return 500; } }
        private readonly Spell.Skillshot Q, W, R;
        private readonly Spell.Targeted E;
        private int minManaHarass = 35;
        private int tick;
        private bool isTearOwned;
        private string dmg;
        public Cassiopeia()
        {
            ShopSequence =
                "3340:Buy,2003:StartHpPot,1056:Buy,1027:Buy,3070:Buy,1001:Buy,1058:Buy,3003:Buy,3020:Buy,1028:Buy,1011:Buy,1058:Buy,2003:StopHpPot,3116:Buy,1004:Buy,1004:Buy,3114:Buy,1052:Buy,3108:Buy,3165:Buy,1056:Sell,1058:Buy,3089:Buy,1028:Buy,3136:Buy,3151:Buy";
            Q = new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Circular, 670, int.MaxValue, 35);
            W = new Spell.Skillshot(SpellSlot.W, 850, SkillShotType.Circular, 600, 2500, 90);
            R = new Spell.Skillshot(SpellSlot.R, 500, SkillShotType.Cone, 650, int.MaxValue, 75);
            E = new Spell.Targeted(SpellSlot.E, 700);
            updateTearStatus();
            Game.OnTick += Game_OnTick;
            if (MainMenu.GetMenu("AB").Get<CheckBox>("debuginfo").CurrentValue)
                Drawing.OnDraw += Drawing_OnDraw;
        }

        private void updateTearStatus()
        {
            isTearOwned = BrutalItemInfo.GetItemSlot(3070) != -1 || BrutalItemInfo.GetItemSlot(3003) != -1;
            Core.DelayAction (updateTearStatus,5000);
        }



        void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawText(900, 10, Color.Chocolate, dmg, 10);
            /*AIHeroClient buf =
    EntityManager.Heroes.AllHeroes.Where(h => h.Distance(Game.CursorPos) < 800)
        .OrderBy(e => e.Distance(Game.CursorPos))
        .FirstOrDefault();
            if (buf != null)
            {
                int y = 0;
                foreach (BuffInstance buff in buf.Buffs)
                {
                    Drawing.DrawText(500, 500 + y, Color.Chocolate, "Name: " +buff.Name+"  DisplayName: " +buff.DisplayName, 10);
                    y += 20;
                }
            }
            

            AIHeroClient t=EntityManager.Heroes.Enemies.FirstOrDefault(en=>en.Distance(Game.CursorPos)<600);
            if (t != null)
            {
                Vector2 pos = Game.CursorPos.WorldToScreen();
                pos.Y -= 200;
                /*int offset = 0;
                foreach (BuffInstance buff in t.Buffs)
                {
                    Drawing.DrawText(pos.X, pos.Y+offset, Color.Aqua, buff.Name+" "+(buff.EndTime-Game.Time));
                    offset += 20;
                }
                
                float ti = TimeForAttack(t, 600);
                Drawing.DrawText(pos.X, pos.Y + 60, Color.Aqua, ti + " " + EstDmg(t, ti) + "  " + (t.Health - EstDmg(t, ti)));
            }*/
        }

        private void Game_OnTick(EventArgs args)
        {
            //Chat.Print(AutoWalker.Recalling());
            AIHeroClient t = EntityManager.Heroes.Enemies.Where(en =>en.IsVisible()&& en.Distance(Game.CursorPos) < 630).OrderBy(en=>en.Health).FirstOrDefault();
            if (t != null)
            {
                float ti = TimeForAttack(t, 630);
                float dm=0;
                if (EstDmg(t, ti) > 0)
                {
                    dm=EstDmg(t, ti);
                }
                if(AutoWalker.Ignite!=null&&AutoWalker.Ignite.IsReady()&&t.Health>dm&&t.Health<dm+(50 + 20*AutoWalker.p.Level))
                    AutoWalker.UseIgnite(t);
                    dmg=dm+", "+(t.Health-dm);
            }

            if(isTearOwned&&Q.IsReady()&&AutoWalker.p.ManaPercent>95&&!AutoWalker.Recalling()&&!EntityManager.Heroes.Enemies.Any(en=>en.Distance(AutoWalker.p)<2000)&&!EntityManager.MinionsAndMonsters.EnemyMinions.Any(min=>min.Distance(AutoWalker.p)<1000))
            {
                Q.Cast((Prediction.Position.PredictUnitPosition(AutoWalker.p, 2000) +
                       new Vector2(RandGen.r.NextFloat(-200, 200), RandGen.r.NextFloat(-200, 200))).To3D());
            }



            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Harass || Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LaneClear)
            {
                if (!EntityManager.Heroes.Enemies.Any(en => en.Distance(AutoWalker.p) < 650 + en.BoundingRadius))
                {
                    if (Q.IsReady() && AutoWalker.p.MaxMana>750&&AutoWalker.p.ManaPercent > 65)
                    {
                        tick++;
                        if (tick % 5 != 0) return;
                        EntityManager.MinionsAndMonsters.FarmLocation f =
                            EntityManager.MinionsAndMonsters.GetCircularFarmLocation(EntityManager.MinionsAndMonsters.GetLaneMinions(radius: 850), 250, 700);
                        if (f.HitNumber >= 4 || (f.HitNumber == 3 && AutoWalker.p.ManaPercent > 80))
                            Q.Cast(f.CastPosition);

                    }

                    if (E.IsReady())
                    {
                        Obj_AI_Minion minionToE = EntityManager.MinionsAndMonsters.GetLaneMinions(radius: 850).FirstOrDefault(min => min.HasBuffOfType(BuffType.Poison) && min.Distance(AutoWalker.p) < min.BoundingRadius + E.Range && Prediction.Health.GetPrediction(min, 100) < AutoWalker.p.GetSpellDamage(min, SpellSlot.E) && Prediction.Health.GetPrediction(min, 100) > 0);
                        if (minionToE != null)
                            E.Cast(minionToE);
                        else if (!EntityManager.Heroes.Enemies.Any(en => en.IsVisible() && en.Distance(AutoWalker.p) < 1200))
                        {
                            minionToE = EntityManager.MinionsAndMonsters.GetLaneMinions(radius: 850).FirstOrDefault(min => min.Distance(AutoWalker.p) < min.BoundingRadius + E.Range && Prediction.Health.GetPrediction(min, 200) < AutoWalker.p.GetSpellDamage(min, SpellSlot.E) && Prediction.Health.GetPrediction(min, 200) > 0);
                            if (minionToE != null)
                                E.Cast(minionToE);
                        }
                    }
                }


                if (AutoWalker.p.ManaPercent < 15) return;
                AIHeroClient poorVictim = TargetSelector.GetTarget(850, DamageType.Magical, addBoundingRadius: true);
                if (poorVictim != null && minManaHarass < AutoWalker.p.HealthPercent)
                {
                    if (Q.IsReady())
                    {
                        PredictionResult pr = Q.GetPrediction(poorVictim);
                        if (pr.HitChancePercent > 35)
                        {
                            Q.Cast(pr.CastPosition);
                        }
                    }
                    if (E.IsReady())
                    {
                        AIHeroClient candidateForE = EntityManager.Heroes.Enemies.Where(
                            en =>
                                en.HasBuffOfType(BuffType.Poison) && en.IsTargetable &&
                                !en.HasBuffOfType(BuffType.SpellImmunity) && !en.HasBuffOfType(BuffType.Invulnerability) &&
                                en.Distance(AutoWalker.p) < en.BoundingRadius + E.Range && !en.IsDead())
                            .OrderBy(en => en.Health / AutoWalker.p.GetSpellDamage(en, SpellSlot.E))
                            .FirstOrDefault();
                        if (candidateForE != null)
                            E.Cast(candidateForE);

                    }

                }
            }
            else if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo || Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Flee)
            {
                AIHeroClient poorVictim = TargetSelector.GetTarget(700, DamageType.Magical, addBoundingRadius: true) ??
                                          TargetSelector.GetTarget(850, DamageType.Magical, addBoundingRadius: true);

                if (poorVictim != null)
                {
                    if (Q.IsReady())
                    {
                        PredictionResult pr = Q.GetPrediction(poorVictim);
                        if (pr.HitChancePercent > 30)
                        {
                            Q.Cast(pr.CastPosition);
                        }

                    }
                    if (E.IsReady() && (poorVictim.HasBuffOfType(BuffType.Poison) || AutoWalker.p.GetSpellDamage(poorVictim, SpellSlot.E) > poorVictim.Health))
                        E.Cast(poorVictim);
                    else if (E.IsReady())
                    {
                        AIHeroClient an = EntityManager.Heroes.Enemies.Where(en => en.HasBuffOfType(BuffType.Poison) && AutoWalker.p.Distance(en) < E.Range + en.BoundingRadius).OrderBy(en => en.Health / AutoWalker.p.GetSpellDamage(en, SpellSlot.E))
                            .FirstOrDefault();
                        if (an != null)
                            E.Cast(an);
                    }
                    if (!poorVictim.HasBuffOfType(BuffType.Poison) && W.IsReady() || poorVictim.Distance(AutoWalker.p) > 650)
                    {
                        PredictionResult pr = W.GetPrediction(poorVictim);
                        if (pr.HitChance >= HitChance.Medium)
                        {
                            W.Cast(pr.CastPosition);
                        }
                    }
                    if (R.IsReady() && poorVictim.HasBuffOfType(BuffType.Poison) && AutoWalker.p.ManaPercent > 35 && poorVictim.Distance(AutoWalker.p) > 200 && poorVictim.Distance(AutoWalker.p) < 600 + poorVictim.BoundingRadius && poorVictim.IsFacing(AutoWalker.p) && poorVictim.HealthPercent > 30 && poorVictim.HealthPercent < 60)
                        R.Cast(Prediction.Position.PredictUnitPosition(poorVictim, 300).To3D());
                    if (R.IsReady() && poorVictim.Distance(AutoWalker.p) < 600&&EntityManager.Heroes.Enemies.Count(en=>en.IsVisible()&&!en.IsDead()&&en.Distance(AutoWalker.p)<600)>=2)
                        R.Cast(Prediction.Position.PredictUnitPosition(poorVictim, 400).To3D());
                    if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Flee)
                    {
                        if (R.IsReady() && Logic.surviLogic.dangerValue > 10000 && AutoWalker.p.HealthPercent < 20)
                        {
                            AIHeroClient champToUlt =
                                EntityManager.Heroes.Enemies.FirstOrDefault(
                                    en =>
                                        en.HealthPercent > 5 && en.Distance(AutoWalker.p) < 600 &&
                                        en.Distance(AutoWalker.p) > 100);
                            if (champToUlt != null)
                            {
                                R.Cast(Prediction.Position.PredictUnitPosition(champToUlt, 500).To3D());
                            }
                        }
                    }
                    
                }
                
            }



                if (R.IsReady()&& AutoWalker.p.HealthPercent < 15)
                {
                    AIHeroClient champToUlt =
    EntityManager.Heroes.Enemies.FirstOrDefault(
        en =>en.Distance(AutoWalker.p) < 700);
                    if (champToUlt != null)
                    {
                        R.Cast(champToUlt);
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

        private static float TimeForAttack(Obj_AI_Base o, float range)
        {
            float time=(range-AutoWalker.p.Distance(o))/(o.MoveSpeed+100-AutoWalker.p.MoveSpeed);
            float time2 = (AutoWalker.p.Distance(o.GetNearestTurret())-950) / (o.MoveSpeed + 100 - AutoWalker.p.MoveSpeed);
            return time < time2 ? time : time2;
        }
        private float EstDmg(Obj_AI_Base o, float time)
        {

            float eCD = E.Handle.CooldownExpires - Game.Time < 0 ? 0 : E.Handle.CooldownExpires - Game.Time;
            float qCD = Q.Handle.CooldownExpires - Game.Time < 0 ? 0 : Q.Handle.CooldownExpires - Game.Time;
            float eTimes = (float) Math.Floor((time - eCD)/.5f);
            float damage = AutoWalker.p.GetSpellDamage(o, SpellSlot.E)*eTimes;
            damage += AutoWalker.p.GetSpellDamage(o, SpellSlot.Q)*(float)Math.Floor((time - qCD)/Q.Handle.Cooldown);
            float neededMana = E.Handle.SData.Mana*eTimes + Q.Handle.SData.Mana;
            if (AutoWalker.p.Mana < neededMana)
                return damage*AutoWalker.p.Mana/neededMana;
            return damage ;
        }
    }
}
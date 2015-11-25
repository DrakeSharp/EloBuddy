using System;
using System.Linq;
using AutoBuddy.Utilities;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;

namespace EssentialMapHack
{
    internal static class BrutalExtensions
    {
        public static Lane GetLane(this Obj_AI_Minion min)
        {
            try
            {
                if (min.Name == null || min.Name.Length < 13) return Lane.Unknown;
                if (min.Name[12] == '0') return Lane.Bot;
                if (min.Name[12] == '1') return Lane.Mid;
                if (min.Name[12] == '2') return Lane.Top;
            }
            catch (Exception e) {Console.WriteLine("GetLane:"+e.Message); }
            return Lane.Unknown;
        }

        public static Lane GetLane(this Obj_AI_Turret tur)
        {
            if (tur.Name.EndsWith("Shrine_A")) return Lane.Spawn;
            if (tur.Name.EndsWith("C_02_A") || tur.Name.EndsWith("C_01_A")) return Lane.HQ;
            if (tur.Name == null || tur.Name.Length < 12) return Lane.Unknown;
            if (tur.Name[10] == 'R') return Lane.Bot;
            if (tur.Name[10] == 'C') return Lane.Mid;
            if (tur.Name[10] == 'L') return Lane.Top;
            return Lane.Unknown;
        }

        public static int GetWave(this Obj_AI_Minion min)
        {
            if (min.Name == null || min.Name.Length < 17) return 0;
            int result;
            try
            {
                result = int.Parse(min.Name.Substring(14, 2));
            }
            catch (FormatException)
            {
                result = 0;
                Console.WriteLine("GetWave error, minion name: " + min.Name);
            }
            return result;
        }

        public static Vector3 RotatedAround(this Vector3 rotated, Vector3 around, float angle)
        {
            double s = Math.Sin(angle);
            double c = Math.Cos(angle);

            Vector2 ret = new Vector2(rotated.X - around.X, rotated.Y - around.Y);

            double xnew = ret.X*c - ret.Y*s;
            double ynew = ret.X*s + ret.Y*c;

            ret.X = (float) xnew + around.X;
            ret.Y = (float) ynew + around.Y;

            return ret.To3DWorld();
        }


        public static Obj_AI_Turret GetNearestTurret(this Vector3 pos, bool enemy = true)
        {
            return
                ObjectManager.Get<Obj_AI_Turret>()
                .Where(tur => tur.Health > 0 && tur.IsAlly^enemy)
                    .OrderBy(tur => tur.Distance(pos))
                    .First();
        }

        public static Obj_AI_Turret GetNearestTurret(this Obj_AI_Base unit, bool enemy = true)
        {
            return unit.Position.GetNearestTurret(enemy); 
        }

        public static bool IsVisible(this Obj_AI_Base unit)
        {
            return !unit.IsDead() && unit.IsHPBarRendered;
        }

        public static bool IsDead(this Obj_AI_Base unit)
        {
            return unit.Health <= 0;
        }
        public static float HealthPercent(this Obj_AI_Base unit)
        {
            return unit.Health/unit.MaxHealth*100f;
        }
    }
}
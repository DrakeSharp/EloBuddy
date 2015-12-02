using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;

namespace EssentialLvlUp
{
    internal static class BrutalExtensions
    {
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

        public static Vector3 Randomized(this Vector3 vec, float min = -300, float max = 300)
        {
            return new Vector3(vec.X + RandGen.r.NextFloat(min, max), vec.Y + RandGen.r.NextFloat(min, max), vec.Z);
        }

        public static Obj_AI_Turret GetNearestTurret(this Vector3 pos, bool enemy = true)
        {
            return
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(tur => tur.Health > 0 && tur.IsAlly ^ enemy)
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

        public static string Concatenate<T>(this IEnumerable<T> source, string delimiter)
        {
            var s = new StringBuilder();
            bool first = true;
            foreach (T t in source)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    s.Append(delimiter);
                }
                s.Append(t);
            }
            return s.ToString();
        }

        public static List<int> AllIndexesOf(string str, string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            List<int> indexes = new List<int>();
            for (int index = 0;; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static string GetResponseText(this string address)
        {
            var request = (HttpWebRequest) WebRequest.Create(address);
            request.Proxy = null;
            using (var response = (HttpWebResponse) request.GetResponse())
            {
                var encoding = Encoding.GetEncoding(response.CharacterSet);

                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream, encoding))
                    return reader.ReadToEnd();
            }
        }
    }
}
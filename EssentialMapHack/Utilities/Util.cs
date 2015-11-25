using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Resources;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Rendering;
using EssentialMapHack.Properties;
using SharpDX;
using Color = System.Drawing.Color;
using Rectangle = SharpDX.Rectangle;

namespace EssentialMapHack.Utilities
{
    internal static class Util
    {
        public static Rectangle MinimapRectangle;
        public static float MinimapMul = -1f;
        public const float PI2 = (float)Math.PI * 2;


        static Util()
        {
            Init();
        }


        public static void Init(EventArgs args = null)
        {
            Core.DelayAction(() =>
            {

                float mmul =
                    new Vector3(1000, 1000, 0).WorldToMinimap().Distance(new Vector3(2000, 1000, 0).WorldToMinimap()) /
                    1000f;
                if (mmul <= 0.0000001)
                    Init();
                else
                {
                    MinimapMul = mmul;
                    Vector2 leftUpper;
                    Vector2 rightLower;
                    if (Game.MapId == GameMapId.CrystalScar)
                    {
                        leftUpper = new Vector3(0, 13800, 0).WorldToMinimap();
                        rightLower = new Vector3(13800, 0, 0).WorldToMinimap();
                    }
                    else
                    {
                        leftUpper = new Vector3(0, 14800, 0).WorldToMinimap();
                        rightLower = new Vector3(14800, 0, 0).WorldToMinimap();
                    }

                    MinimapRectangle = new Rectangle((int)leftUpper.X, (int)leftUpper.Y,
                        (int)(rightLower.X - leftUpper.X), (int)(rightLower.Y - leftUpper.Y));
                }

            }, 1000);
        }

        public static void Draw2DCircle(Vector2 position, float radius, System.Drawing.Color color, float width = 2F,
            int quality = -1)
        {
            if (quality == -1)
                quality = (int)(radius / 7 + 11);
            Vector2[] points = new Vector2[quality + 1];
            Vector2 rad = new Vector2(0, radius);

            for (int i = 0; i <= quality; i++)
            {
                points[i] = (position + rad).RotateAroundPoint(position, PI2 * i / quality);
            }
            Line.DrawLine(color, width, points);

        }

        public static void DrawArc(Vector2 position, float radius, Color color, float startDegree,
            float length, float width = 0.6F,
            int quality = -1)
        {

            if (quality == -1)
                quality = (int)(radius / 7 + 11);
            Vector2[] points = new Vector2[(int)(Math.Abs(quality * length / PI2) + 1)];
            Vector2 rad = new Vector2(0, radius);


            for (int i = 0; i <= (int)(Math.Abs(quality * length / PI2)); i++)
            {
                points[i] = (position + rad).RotateAroundPoint(position,
                    startDegree + PI2 * i / quality * (length > 0 ? 1 : -1));
            }
            Line.DrawLine(color, width, points);

        }

        public static void DrawCricleMinimap(Vector3 mapPosition, float radius, Color color,
            float width = 2F,
            int quality = -1)
        {
            DrawCricleMinimap(mapPosition.WorldToMinimap(), radius * MinimapMul, color, width, quality);

        }

        public static void DrawCricleMinimap(Vector2 screenPosition, float radius, Color color,
            float width = 2F,
            int quality = -1)
        {
            if (quality == -1)
                quality = (int)(radius / 3 + 15);

            Vector2 rad = new Vector2(0, radius);
            List<MinimapCircleSegment> segments = new List<MinimapCircleSegment>();
            bool full = true;
            for (int i = 0; i <= quality; i++)
            {
                Vector2 pos = (screenPosition + rad).RotateAroundPoint(screenPosition, PI2 * i / quality);
                bool contains = MinimapRectangle.Contains(pos);
                if (!contains)
                    full = false;
                segments.Add(new MinimapCircleSegment(pos, contains));
            }

            foreach (Vector2[] ar in FindArcs(segments, full))
                Line.DrawLine(color, width, ar);

        }

        public static void Draw(this Rectangle rect, Color color, float width = 2F)
        {
            Line.DrawLine(color, width, rect.BottomLeft, rect.BottomRight, rect.TopRight, rect.TopLeft, rect.BottomLeft);

        }

        public static List<Vector2[]> FindArcs(List<MinimapCircleSegment> points, bool full)
        {
            List<Vector2[]> ret = new List<Vector2[]>();
            if (full)
            {
                ret.Add(points.Select(segment => segment.pos).ToArray());
                return ret;
            }
            int pos = 0;
            for (int c = 0; c < 3; c++)
            {
                int start = -1, stop = -1;
                for (int i = pos; i < points.Count; i++)
                {

                    if (points[i].ok)
                    {
                        if (start == -1)
                            start = i;
                    }
                    else
                    {
                        if (stop == -1 && start != -1)
                        {
                            stop = i;
                            pos = i;
                            if (start != 0) break;
                            for (int j = points.Count - 1; j > 0; j--)
                            {
                                if (points[j].ok)
                                    start = j;
                                else
                                    break;
                            }
                        }
                        if (i == points.Count - 1) pos = points.Count;
                    }

                }
                List<Vector2> arc = new List<Vector2>();

                if (start == -1 || stop == -1)
                    continue;
                int pointer = start;

                while (true)
                {
                    if (pointer == stop)
                        break;
                    arc.Add(points[pointer].pos);
                    pointer++;
                    if (pointer == points.Count)
                        pointer = 0;
                }

                ret.Add(arc.ToArray());

            }

            return ret;
        }

        public static Bitmap cropAtRect(Bitmap bi, int size)
        {
            Bitmap btm = new Bitmap(bi.Width + 4, bi.Height + 4);
            Bitmap btm2 = new Bitmap(size + 5, size + 5);

            using (Graphics grf = Graphics.FromImage(bi))
            {
                using (Brush brsh = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
                {
                    grf.FillRectangle(brsh, new System.Drawing.Rectangle(0, 0, bi.Width, bi.Height));
                }
            }
            using (Graphics grf = Graphics.FromImage(btm))
            {
                using (Brush brsh = new SolidBrush(Color.Red))
                {
                    grf.FillEllipse(brsh, 2, 2, bi.Width - 4, bi.Height - 4);
                }
                using (Brush brsh = new TextureBrush(bi))
                {

                    grf.FillEllipse(brsh, 6, 6, bi.Width - 12, bi.Height - 12);
                }
            }
            using (Graphics grf = Graphics.FromImage(btm2))
            {
                grf.InterpolationMode = InterpolationMode.High;
                grf.CompositingQuality = CompositingQuality.HighQuality;
                grf.SmoothingMode = SmoothingMode.AntiAlias;
                grf.DrawImage(btm, new System.Drawing.Rectangle(0, 0, size, size));
            }

            return btm2;


        }

        public static Sprite CreateChampTexture(string name, int spriteSize)
        {
            Bitmap bit = null;
            try
            {
                ResourceSet resourceSet = Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
                foreach (DictionaryEntry entry in resourceSet)
                {
                    string resourceKey = (string)entry.Key;
                    if (resourceKey.ToLower().Equals(name.ToLower() + "_square_0"))
                        bit = (Bitmap)entry.Value;
                }
                if (bit == null) throw new Exception("File not found");
            }
            catch (Exception)
            {
                bit = Resources.Unknown;
            }
            return new Sprite(TextureLoader.BitmapToTexture(cropAtRect(bit, spriteSize)));
        }

        public static Vector3 GetSpawn(AIHeroClient hero)
        {
            var firstOrDefault = ObjectManager.Get<Obj_Building>().FirstOrDefault(spaw => spaw.Team.Equals(hero.Team) && spaw.Name.StartsWith("__Spawn"));
            return firstOrDefault != null ? firstOrDefault.Position : new Vector3();
        }

        public static Color PercentToRGB(float percent, int a = 255)
        {
            if (percent >= 100)
                percent = 99.99f;
            int r, g;
            if (percent < 50)
            {
                r = (int)Math.Floor(255 * (percent / 50));
                g = 255;
            }
            else
            {
                r = 255;
                g = (int)Math.Floor(255 * ((50 - percent % 50) / 50));
            }
            return Color.FromArgb(a, g, r, 0);

        }

        /// <summary>
        /// Warning:changes "from" parameter
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        public static Vector2 PosBetween(Vector2 from, Vector2 to, float percent)
        {
            from.X -= (to.X - from.X) * percent;
            from.Y -= (to.Y - from.Y) * percent;
            return from;
        }
        public static Vector3 PosBetween(Vector3 from, Vector3 to, float percent)
        {
            from.X += (to.X - from.X) * percent;
            from.Y += (to.Y - from.Y) * percent;
            return from;
        }
    }
}

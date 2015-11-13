using System;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using Color = System.Drawing.Color;

namespace ObjectBuddy
{
    internal class ObjectSelector
    {
        private GameObject[] nearbyObjects = new GameObject[0];
        private GameObject selectedObject;

        private readonly KeyBind kb, refresh;
        private readonly Menu menuTypes;
        private readonly Slider range;
        private readonly Text display;


        private string proper = " ";
        private string proper2 = " ";
        private string proper3 = " ";
        private string proper4 = " ";
        private string val = " ";
        private string val2 = " ";
        private string val3 = " ";
        private string val4 = " ";

        public ObjectSelector()
        {
            display = new Text(" ", new Font(FontFamily.GenericSansSerif, 8, FontStyle.Regular)) { Color = Color.Chartreuse };
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Menu menu = MainMenu.AddMenu("ObjectBuddy", "lol");
            Menu menuGeneral = menu.AddSubMenu("General settings", "gen");
            range = menuGeneral.Add("ran", new Slider("Detection range", 300, 0, 6000));
            kb = menuGeneral.Add("key", new KeyBind("Select object", false, KeyBind.BindTypes.HoldActive));
            refresh = menuGeneral.Add("ref", new KeyBind("Refresh info", false, KeyBind.BindTypes.PressToggle));
            kb.OnValueChange += kb_OnValueChange;
            menuTypes = menu.AddSubMenu("Object types", "typ");
            foreach (GameObjectType type in Enum.GetValues(typeof(GameObjectType)))
            {
                menuTypes.Add(type.ToString(), new CheckBox(type.ToString()));
            }


        }


        private void kb_OnValueChange(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            if (args.NewValue)
            {
                selectedObject = null;
            }
            else if (nearbyObjects.Any())
            {
                selectedObject = nearbyObjects.First();
                RefreshText();
            }
        }

        private void Drawing_OnEndScene(EventArgs args)
        {
            if (kb.CurrentValue)
            {
                Drawing.DrawCircle(Game.CursorPos, range.CurrentValue, Color.BurlyWood);
                foreach (GameObject obj in nearbyObjects)
                {
                    Drawing.DrawCircle(obj.Position, 40, Color.Aqua);

                }

                if (nearbyObjects.Any())
                    Line.DrawLine(Color.Red, Game.CursorPos.WorldToScreen(),
                        nearbyObjects.First().Position.WorldToScreen());
            }

            if (selectedObject == null) return;
            display.Draw(proper, Color.Chartreuse, 10, 10);
            display.Draw(val, Color.Chartreuse, 230, 10);

            display.Draw(proper2, Color.Chartreuse, 400, 10);
            display.Draw(val2, Color.Chartreuse, 620, 10);

            display.Draw(proper3, Color.Chartreuse, 800, 10);
            display.Draw(val3, Color.Chartreuse, 1020, 10);

            display.Draw(proper4, Color.Chartreuse, 1100, 60);
            display.Draw(val4, Color.Chartreuse, 1320, 60);
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (kb.CurrentValue)
            {
                nearbyObjects =
                    ObjectManager.Get<GameObject>()
                        .Where(
                            ob =>
                                ob.Position.Distance(Game.CursorPos) < range.CurrentValue &&
                                menuTypes.Get<CheckBox>(ob.Type.ToString()).CurrentValue)
                        .OrderBy(ob => ob.Position.Distance(Game.CursorPos))
                        .ToArray();
            }
            if(refresh.CurrentValue && selectedObject != null && ObjectManager.GetUnitByNetworkId((uint) selectedObject.NetworkId) != null)
                RefreshText();
        }

        private void RefreshText()
        {
            proper = "";
            proper2 = "";
            proper3 = "";
            proper4 = "";
            val = "";
            val2 = "";
            val3 = "";
            val4 = "";
            int count = 0;
            foreach (var prop in nearbyObjects.First().GetType().GetProperties().OrderBy(q => q.ToString()).ToList().Where(prop => nearbyObjects.First().Type != GameObjectType.AIHeroClient || (!prop.Name.Contains("Kills") && !prop.Name.Contains("Wards") && !prop.Name.Contains("Killed") && !prop.Name.Contains("Nodes") && !prop.Name.Contains("gest") && !prop.Name.Contains("Dealt") && !prop.Name.Contains("Taken") && !prop.Name.Contains("Score") && !prop.Name.Contains("Total") && !prop.Name.Equals("Assists") && !prop.Name.Equals("Deaths"))).Where(prop => !prop.Name.Equals("BBox")))
            {
                count++;
                if (count < 69)
                {
                    proper += prop.Name + " :\n";
                    val += prop.GetValue(nearbyObjects.First(), null) + "\n";
                }
                else if (count < 129)
                {
                    proper2 += prop.Name + " :\n";
                    val2 += prop.GetValue(nearbyObjects.First(), null) + "\n";
                }
                else if (count < 189)
                {
                    proper3 += prop.Name + " :\n";
                    val3 += prop.GetValue(nearbyObjects.First(), null) + "\n";
                }
                else
                {
                    proper4 += prop.Name + " :\n";
                    val4 += prop.GetValue(nearbyObjects.First(), null) + "\n";
                }
            }
        }
    }
}

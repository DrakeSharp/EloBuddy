using System;
using System.Management;
using EloBuddy;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace BrightBuddy
{
    internal class Program
    {
        private static ManagementClass mclass;
        private static ManagementObjectCollection instances;

        private static void Main()
        {
            mclass = new ManagementClass("WmiMonitorBrightnessMethods") {Scope = new ManagementScope(@"\\.\root\wmi")};
            instances = mclass.GetInstances();
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            var menu = MainMenu.AddMenu("BrightBuddy", "ffs");
            var setbright = new Slider("Screen brightness", 50);
            menu.Add("ff", setbright);
            setbright.OnValueChange += setbright_OnValueChange;
            if (menu.Add("sh", new CheckBox("Apply on game load", false)).CurrentValue)
                SetBrightness(setbright.CurrentValue);
            menu.AddGroupLabel("By Christian Brutal Sniper");
        }

        private static void setbright_OnValueChange(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
        {
            SetBrightness(args.NewValue);
        }

        private static void SetBrightness(int brightness)
        {
            try
            {
                foreach (ManagementObject instance in instances)
                {
                    instance.InvokeMethod("WmiSetBrightness", new object[] {0, brightness});
                }
            }
            catch (Exception e)
            {
                Chat.Print("BrightBuddy: something doesn't work.\n" + e);
            }
        }
    }
}
using System;
using System.IO;
using EloBuddy;
using EloBuddy.SDK.Events;
using EssentialLvlUp.AutoLvl;

namespace EssentialLvlUp
{
    internal class Program
    {
        private static void Main()
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "EssentialLvlUp"));

            new CustomLvlSeq(null, ObjectManager.Player, Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "EssentialLvlUp"));
        }
    }
}
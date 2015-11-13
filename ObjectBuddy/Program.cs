using System;
using EloBuddy.SDK.Events;

namespace ObjectBuddy
{
    class Program
    {
        private static void Main()
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
            
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            new ObjectSelector();
        }
    }
}

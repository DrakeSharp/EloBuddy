using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace AutoBuddy.Utilities
{
    static class Telemetry
    {
        private static string id;
        private static string dir;
        static Telemetry()
        {

        }

        public static void Init(string directory)
        {
            dir = directory;
            setId();
        }

        private static void setId()
        {
            if (!File.Exists(dir + "\\profile"))
                getId();
            else
            {
                string content = File.ReadAllText(dir + "\\profile");
                if(content.Equals(string.Empty))
                    getId();
                id = content;
            }
        }
        public static void SendEvent(string type, string data)
        {
            BackgroundWorker bw3 = new BackgroundWorker();
            bw3.DoWork += bw3_DoWork;
            bw3.RunWorkerAsync(new[] { type, data });
        }

        private static void bw3_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] args = (string[])e.Argument;
            string x =
                "http://autobuddy.tk/ann/d.php".Post(new Dictionary<string, string>()
                {
                    {"id", id},
                    {"type", "event"},
                    {"data", args[0] + ":" + args[1]}
                });

            while(!x.Contains("thanks"))
            {
                if(x.Contains("error 1"))
                    getId();
                Thread.Sleep(5000);
                x =
                "http://autobuddy.tk/ann/d.php".Post(new Dictionary<string, string>()
                {
                    {"id", id},
                    {"type", "event"},
                    {"data", args[0] + ":" + args[1]}
                });
                
            }

        }

        public static void SendFileAndDelete(string file, string name)
        {
            BackgroundWorker bw2 = new BackgroundWorker();
            bw2.DoWork += bw2_DoWork;
            bw2.RunWorkerAsync(new[]{file, name});
        }

        private static void bw2_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] args = (string[]) e.Argument;
            if (!File.Exists(args[0])) return;
            string result = "http://autobuddy.tk/ann/d.php".Post(new Dictionary<string, string>() { { "id", id }, { "type", args[1] }, { "data", File.ReadAllText(args[0]) } });
            if(result.Contains("thanks"))
                File.Delete(args[0]);
        }


        private static void getId()
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;

            bw.RunWorkerAsync();
        }
        private static void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result="http://autobuddy.tk/ann/d.php".Post(new Dictionary<string, string>() {{"type", "getID"}});
        }

        private static void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            id = (string) e.Result;
            File.WriteAllText(dir + "\\profile", id);
        }


    }
}

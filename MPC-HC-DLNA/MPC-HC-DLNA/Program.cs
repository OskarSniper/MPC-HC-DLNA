using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.Win32;

namespace MPC_HC_DLNA
{
    class Program
    {
        static void Main(string[] args)
        {
            WindowsIdentity ident = WindowsIdentity.GetCurrent();
            WindowsPrincipal winprin = new WindowsPrincipal(ident);
            if (!winprin.IsInRole(WindowsBuiltInRole.Administrator)) { Console.WriteLine("Please run as administrator"); return; }

            Console.Write("DLNA IP: ");
            string ip = Console.ReadLine();

            //TODO: Add parameters to request ?
            Console.Write("Parameter ? (like ?transcode=internet360):");
            string param = Console.ReadLine();

            // Request Homerun DLNA Service
            WebRequest request = WebRequest.Create("http://" + ip + "/lineup.json");

            // Get response
            WebResponse response = request.GetResponse();
            Stream data = response.GetResponseStream();
            string html = String.Empty;

            using (StreamReader sr = new StreamReader(data))
            {
                html = sr.ReadToEnd();
                var list = JsonConvert.DeserializeObject<List<ProgramList>>(html);
                int i = 0;
                string runKey = "SOFTWARE\\MPC-HC\\MPC-HC";
                using (RegistryKey Key = Registry.CurrentUser.OpenSubKey(runKey))
                {
                    RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\MPC-HC\\MPC-HC\\Favorites\\Files");
                    //Key.CreateSubKey("Favorites\\Files");
                }
                foreach (var item in list)
                {
                    RegistryKey startupKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\MPC-HC\\MPC-HC\\Favorites\\Files", true);
                    startupKey.SetValue("Name" + i, item.GuideName + ";0;0;" + item.URL + param, RegistryValueKind.String);

                    i++;
                }
            }
            Console.WriteLine("Finished import ;-)");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public class ProgramList
        {
            public string GuideNumber { get; set; }
            public string GuideName { get; set; }
            public string URL { get; set; }
        }
    }
}

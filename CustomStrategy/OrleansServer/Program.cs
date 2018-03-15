using GrainsLibrary;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OrleansServer
{
    class Program
    {
        static List<SiloHost> silos;
        static void Main(string[] args)
        {
            silos = new List<SiloHost>();
            RunSilo(1);
            RunSilo(2);
            RunSilo(3);

            Console.ReadLine();
            ShutDown();
        }

        static void RunSilo(int number)
        {
            var config = new ClusterConfiguration();
            config.LoadFromFile($"../../OrleansConfiguration{number}.xml");
            config.UseStartupType<Startup>();
            config.Globals.DeploymentId = "Silo";
            var silo = new SiloHost($"Silo {number}", config);

            silo.InitializeOrleansSilo();
            silo.StartOrleansSilo();
            silos.Add(silo);
            Console.WriteLine($"Silo {number} is ready.");
        }

        static void ShutDown()
        {
            if (silos != null)
                foreach (var silo in silos)
                    silo?.ShutdownOrleansSilo();
        }
    }
}

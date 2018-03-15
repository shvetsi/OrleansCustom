using GrainsLibrary;
using Orleans;
using Orleans.Runtime.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrleansClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = ClientConfiguration.LocalhostSilo();
            var client = new ClientBuilder()
               .LoadConfiguration("../../ClientConfiguration.xml")
               .Build();
            client.Connect().Wait();
            Console.WriteLine("Press number or x [Enter] to terminate...");

            string nextCommand = "";
            while (!(nextCommand.ToUpperInvariant() == "X"))
            {
                nextCommand = Console.ReadLine();
                var grain = client.GetGrain<ITalkativeGrain>("Grain-" + nextCommand);
                grain.Talk("Grain-" + nextCommand);
            }

            client.Close();
            client.Dispose();
        }
    }
}

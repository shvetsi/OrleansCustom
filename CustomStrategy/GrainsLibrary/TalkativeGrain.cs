using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrainsLibrary
{
    [CountBasedStrategy]
    public class TalkativeGrain : Grain, ITalkativeGrain
    {
        public Task Talk(string name)
        {
            Console.WriteLine($"Talkative {name} is remarkable for intelligence and ingenuity.");

            return Task.CompletedTask;
        }
    }
}

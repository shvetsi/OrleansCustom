using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrainsLibrary
{
    public interface ITalkativeGrain : IGrainWithStringKey
    {
        Task Talk(string name);
    }
}

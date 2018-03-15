using Microsoft.Extensions.DependencyInjection;
using Orleans.Placement;
using Orleans.Runtime;
using Orleans.Runtime.Placement;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrainsLibrary
{
    [Serializable]
    public class CountBasedPlacementStrategy : PlacementStrategy
    {
        internal static CountBasedPlacementStrategy Singleton { get; } = new CountBasedPlacementStrategy();
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CountBasedStrategyAttribute : PlacementAttribute
    {
        public CountBasedStrategyAttribute() : base(CountBasedPlacementStrategy.Singleton)
        {
        }
    }


    public class Startup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IPlacementDirector<CountBasedPlacementStrategy>, CustomActivationDirector>();
            return services.BuildServiceProvider();
        }
    }

    class CustomActivationDirector : IPlacementDirector<CountBasedPlacementStrategy>
    {
        static ConcurrentDictionary<SiloAddress, int> siloCache = new ConcurrentDictionary<SiloAddress, int>();

        //TODO: get rid of dead activations. Somehow.
        public virtual Task<SiloAddress> OnAddActivation(
            PlacementStrategy strategy, PlacementTarget target, IPlacementContext context)
        {
            var allSilos = context.GetCompatibleSilos(target);
            var incompatibleSilos = new List<SiloAddress>();

            SiloAddress leastLoadedSilo = GetLeastLoadedCompatibleSilo(allSilos, incompatibleSilos);

            //Remove incompatible silos
            int value = 0;
            foreach (var silo in incompatibleSilos)
                siloCache.TryRemove(silo, out value);

            UpdateCache(leastLoadedSilo);            

            return Task.FromResult(leastLoadedSilo);
        }

        /// <summary>
        /// Calculates least loaded silo and checks cache for compatibility
        /// </summary>
        /// <param name="allSilos"></param>
        /// <param name="incompatibleSilos"></param>
        /// <returns></returns>
        SiloAddress GetLeastLoadedCompatibleSilo(IList<SiloAddress> allSilos, IList<SiloAddress> incompatibleSilos)
        {
            var min = int.MaxValue;
            SiloAddress leastLoadedSilo = null;
            foreach (var silo in siloCache)
            {
                //If allSilos doesn't contain address from siloCache, it is incompatible
                if (!allSilos.Contains(silo.Key))
                {
                    incompatibleSilos.Add(silo.Key);
                }
                else //looking for silo with minimum activations
                {
                    int activations = silo.Value;
                    if (activations < min)
                    {
                        min = activations;
                        leastLoadedSilo = silo.Key;
                    }
                }
                //Here we are calculating the difference of two sets. 
                allSilos.Remove(silo.Key);
            }

            //If at the end of the cycle allSilos contains items, 
            //we still have unused silos and can mark one of them as a candidate for activation.
            if (allSilos.Count > 0)
                leastLoadedSilo = allSilos[0];

            if (leastLoadedSilo == null)
                throw new OrleansException("No compatible grain");

            return leastLoadedSilo;
        }

        void UpdateCache(SiloAddress siloForActivation)
        {
            //get number of activations and increment it
            int value = 0;
            siloCache.TryGetValue(siloForActivation, out value);
            value++;
            siloCache.AddOrUpdate(siloForActivation, value, (oldkey, oldvalue) => value);
        }
    }
}

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
        static ConcurrentQueue<SiloAddress> siloQueue = new ConcurrentQueue<SiloAddress>();

        public virtual Task<SiloAddress> OnAddActivation(
            PlacementStrategy strategy, PlacementTarget target, IPlacementContext context)
        {
            if (siloQueue.Count == 0)
                siloQueue = new ConcurrentQueue<SiloAddress>(context.GetCompatibleSilos(target));

            SiloAddress leastLoadedSilo = null;
            //Choose first silo in queue
            siloQueue.TryDequeue(out leastLoadedSilo);

            if (leastLoadedSilo == null)
                throw new OrleansException("No compatible silo");

            //and move it to end
            siloQueue.Enqueue(leastLoadedSilo);

            return Task.FromResult(leastLoadedSilo);
        }
    }
}

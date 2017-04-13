using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Common;
using System.Fabric;

namespace Particle
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.None)]
    internal class Particle : Actor, IParticle
    {
        /// <summary>
        /// Initializes a new instance of Particle
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public Particle(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }
        public Task DestermineLocation(double x, double y, Guid aggreator, long index)
        {
            bool inside = (x * x + y * y) <= 0.25;

            System.Diagnostics.Debug.WriteLine($"particle {index}");

            ActorId id = new ActorId(aggreator);
            var fc = new FabricClient();


            var serviceUri = new Uri(FabricRuntime.GetActivationContext().ApplicationName + "/AggreatorActorService");

            IAggreator proxy = ActorProxy.Create<IAggreator>(id, serviceUri);
            return proxy.Report(inside, index);
        }

        
    }
}

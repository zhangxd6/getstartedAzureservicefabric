using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Common;

namespace Aggreator
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class Aggreator : Actor, IAggreator
    {
        /// <summary>
        /// Initializes a new instance of Aggreator
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public Aggreator(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public async Task<Reslut> GetResult()
        {
            var current = await this.StateManager.GetStateAsync<long>("count");
            var process = await this.StateManager.GetStateAsync<long>("process");
            var total = await this.StateManager.GetStateAsync<long>("total");
            Reslut r = new Reslut
            {
                Result = 4.0 * (current) / (process),
                Status = process == total ? "Done" : "Processing",
                Total = total,
                Processed = process
            };
            return r;
        }

        public async Task Init(long count)
        {
            await this.StateManager.TryAddStateAsync<long>("process", 0);
            await this.StateManager.TryAddStateAsync<long>("count", 0);
            await this.StateManager.TryAddStateAsync<long>("total", count);
        }

        public async Task Report(bool status, long index)
        {
            System.Diagnostics.Debug.WriteLine($"aggreator {index}");

            var current = await this.StateManager.GetStateAsync<long>("count");
            if (status)
            {
                await this.StateManager.SetStateAsync<long>("count", current + 1);
            }
            var process = await this.StateManager.GetStateAsync<long>("process");
            await this.StateManager.SetStateAsync<long>("process", process + 1);
        }
    }
}

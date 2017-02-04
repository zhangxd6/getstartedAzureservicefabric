using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
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
      var current = await this.StateManager.TryGetStateAsync<long>("count");
      var process = await this.StateManager.TryGetStateAsync<long>("process");
      var total = await this.StateManager.TryGetStateAsync<long>("total");
      Reslut r = new Reslut
      {
        Result = 4.0 * (current.Value) / (total.Value),
        Status = process.Value == total.Value ? "Done" : "Processing",
        Total = total.Value,
        Processed = process.Value
      };
      return r;
    }

    public async Task Init(long count)
    {
      await this.StateManager.TryAddStateAsync<long>("process", 0);
      await this.StateManager.TryAddStateAsync<long>("count", 0);
      await this.StateManager.TryAddStateAsync<long>("total", count);
    }

    public async Task Report(bool status)
    {
      var current = await this.StateManager.TryGetStateAsync<long>("count");
      if (status)
      {
        long count = current.HasValue ? current.Value : 0;
        count += 1;
        await this.StateManager.SetStateAsync<long>("count", count);
      }
      var process = await this.StateManager.TryGetStateAsync<long>("process");
      long processCount = current.HasValue ? current.Value : 0;
      processCount += 1;
      await this.StateManager.SetStateAsync<long>("process", processCount);
    }


    
  }
}

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Common;
using Microsoft.ServiceFabric.Services.Remoting.Wcf.Runtime;

namespace Stateful1
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class Stateful1 : StatefulService,IStateful
    {
        public Stateful1(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
       {
        new ServiceReplicaListener(context=> new WcfServiceRemotingListener(context,this,null,"ServiceEndpoint"))
      };
        }


        public async Task AddNewContent(string content)
        {
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>("myDictionary");
            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = await myDictionary.TryGetValueAsync(tx, "content");
                var value = $" {result.Value} {Environment.NewLine} {DateTime.Now} {content}";
                await myDictionary.AddOrUpdateAsync(tx, "content", value, (k, v) => value);
                await tx.CommitAsync();
            }
        }

        public async Task<string> GetStoredContent()
        {
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>("myDictionary");
            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = await myDictionary.TryGetValueAsync(tx, "content");
                return result.HasValue ? result.Value : string.Empty;
            }
        }

    }
}

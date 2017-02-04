using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Client;
using System.Linq;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Wcf.Client;
using Common;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;

namespace WebApi.Controllers
{
  [ServiceRequestActionFilter]
  [RoutePrefix("values")]
  public class ValuesController : ApiController
  {
    // GET api/values 
    [HttpGet]
    [Route("")]
    public IEnumerable<string> GetValues()
    {
      return new string[] { "value1", "value2", Environment.MachineName };
    }


    [HttpGet]
    [Route("guest")]
    public async Task<IHttpActionResult> GetAsciiArt(string text)
    {

      var fc = new FabricClient();


      var serviceUri = new Uri(FabricRuntime.GetActivationContext().ApplicationName + "/Guest2");


      var app = await fc.QueryManager.GetApplicationListAsync();
      var service = await fc.QueryManager.GetServiceListAsync(new Uri(FabricRuntime.GetActivationContext().ApplicationName));

      var partitions = await fc.QueryManager.GetPartitionListAsync(serviceUri);
      var partition = (partitions.First()?.PartitionInformation as SingletonPartitionInformation);
      string content = string.Empty;
      ServicePartitionClient<HttpCommunicationClient> partitionClient
                        = new ServicePartitionClient<HttpCommunicationClient>(
                          new HttpCommunicationClientFactory(new ServicePartitionResolver(() => fc)), serviceUri);

      await partitionClient.InvokeWithRetryAsync(
                        async (client) =>
                        {
                          var url = client.Url;
                          if (client.Url.Scheme.ToLower() != "http")
                          {
                            url = new Uri($"http://{client.Url.AbsoluteUri}");
                          }
                          HttpResponseMessage response = await client.HttpClient.GetAsync(new Uri(url, "?cmd=instance"));
                          content = await response.Content.ReadAsStringAsync();

                        });
      return Ok(content);
    }


    [HttpGet]
    [Route("stateful")]
    public async Task<IHttpActionResult> GetStatefulContent()
    {
      var fc = new FabricClient();


      var serviceUri = new Uri(FabricRuntime.GetActivationContext().ApplicationName + "/Stateful1");
      var partitions = await fc.QueryManager.GetPartitionListAsync(serviceUri);
      if (partitions.Count() == 0)
        return BadRequest();
      var partition = (partitions.First()?.PartitionInformation as Int64RangePartitionInformation);
      if (partition != null)
      {
        var factory = new ServiceProxyFactory(c => new WcfServiceRemotingClientFactory());
        var proxy = factory.CreateServiceProxy<IStateful>(serviceUri, new ServicePartitionKey(partition.LowKey));
        var result = await proxy.GetStoredContent();
        return Ok(result);
      }




      return BadRequest();
    }


    [HttpPost]
    [Route("stateful")]
    public async Task<IHttpActionResult> AddContent(string content)
    {
      var fc = new FabricClient();


      var serviceUri = new Uri(FabricRuntime.GetActivationContext().ApplicationName + "/Stateful1");
      var partitions = await fc.QueryManager.GetPartitionListAsync(serviceUri);
      if (partitions.Count() == 0)
        return BadRequest();
      var partition = (partitions.First()?.PartitionInformation as Int64RangePartitionInformation);
      if (partition != null)
      {
        var factory = new ServiceProxyFactory(c => new WcfServiceRemotingClientFactory());
        var proxy = factory.CreateServiceProxy<IStateful>(serviceUri, new ServicePartitionKey(partition.LowKey));

        await proxy.AddNewContent(content);

        return Ok();
      }




      return BadRequest();

    }


    [HttpPost]
    [Route("pi")]
    public async Task<IHttpActionResult> CalculatePi(long number)
    {
      try
      {
        var fc = new FabricClient();
        var serviceUri = new Uri(FabricRuntime.GetActivationContext().ApplicationName + "/AggreatorActorService");
        Guid id = Guid.NewGuid();
        ActorId aid = new ActorId(id);
        var aproxy = ActorProxy.Create<IAggreator>(aid, serviceUri);
        await aproxy.Init(number);
        var pUri = new Uri(FabricRuntime.GetActivationContext().ApplicationName + "/ParticleActorService");
        var rnd = new Random();
         for (long i = 0; i < number; i++)
        //Parallel.For(0, number, async (i) =>
           {
             ActorId pid = new ActorId(Guid.NewGuid());
             var pproxy = ActorProxy.Create<IParticle>(pid, pUri);
             var x = rnd.NextDouble() - 0.5;
             var y = rnd.NextDouble() - 0.5;
             await pproxy.DestermineLocation(x, y, id);
             await Task.Delay(new TimeSpan(0, 0,0,0,300));
           }
        return Ok(id);
      }
      catch
      {
        return BadRequest();
      }
    }


    [HttpGet]
    [Route("pi")]
    public async Task<IHttpActionResult> GetPi(Guid id)
    {
      try
      {
        var fc = new FabricClient();
        var serviceUri = new Uri(FabricRuntime.GetActivationContext().ApplicationName + "/AggreatorActorService");
        ActorId aid = new ActorId(id);
        var aproxy = ActorProxy.Create<IAggreator>(aid, serviceUri);
        var result = await aproxy.GetResult();
        return Ok(result);
      }
      catch
      {
        return BadRequest();
      }
    }

  }
}

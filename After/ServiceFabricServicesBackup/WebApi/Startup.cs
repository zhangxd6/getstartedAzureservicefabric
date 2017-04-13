using System.Web.Http;
using Owin;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.FileSystems;
using System.Net.Http.Formatting;
using Newtonsoft.Json.Serialization;
using System.Linq;
using Swashbuckle.Application;

namespace WebApi
{
  public static class Startup
  {
    // This code configures Web API. The Startup class is specified as a type
    // parameter in the WebApp.Start method.
    public static void ConfigureApp(IAppBuilder appBuilder)
    {
      // Configure Web API for self-host. 
      HttpConfiguration config = new HttpConfiguration();


      config.MapHttpAttributeRoutes();
      config.Routes.MapHttpRoute(
          name: "DefaultApi",
          routeTemplate: "api/{controller}/{id}",
          defaults: new { id = RouteParameter.Optional }
          );

      var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
      jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
      jsonFormatter.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
      appBuilder.UseWebApi(config);
      config
.EnableSwagger(c => c.SingleApiVersion("v1", "A title for your API"))
.EnableSwaggerUi();

      appBuilder.UseFileServer(new FileServerOptions()
      {
        EnableDefaultFiles = true,
        FileSystem = new PhysicalFileSystem(".")
      });
    }
  }
}

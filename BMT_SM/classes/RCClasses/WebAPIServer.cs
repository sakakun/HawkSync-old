using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace HawkSync_SM
{
    class StartUpWebAPI
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/v1/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());

            app.UseWebApi(config);
        }
    }


    public class WebAPIServer
    {
        public void Start()
        {
            // start WebAPI
            ProgramConfig.WebAPI = WebApp.Start<StartUpWebAPI>(new StartOptions()
            {
                Port = 7001
            });
            Console.WriteLine(">> WebAPI Server Started!");
        }
        public void Stop()
        {
            ProgramConfig.WebAPI.Dispose();
            Console.WriteLine(">> WebAPI Server Stopped!");
        }
    }
}

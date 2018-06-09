namespace Dsp.Web
{
    using System.Net.Http.Headers;
    using System.Web.Http;
    using WebApiThrottle;

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.MessageHandlers.Add(new ThrottlingHandler()
            {
                // Generic rate limit applied to ALL APIs
                Policy = new ThrottlePolicy(perSecond: 3, perMinute: 20, perHour: 200)
                {
                    IpThrottling = true,
                    ClientThrottling = true,
                    EndpointThrottling = true
                },
                Repository = new MemoryCacheRepository()
            });

            // WebAPI when dealing with JSON & JavaScript!
            // Setup json serialization to serialize classes to camel (std. Json format)
            var formatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            formatter.SerializerSettings.ContractResolver =
                new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
        }
    }
}
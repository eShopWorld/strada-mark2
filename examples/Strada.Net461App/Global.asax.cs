﻿using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Newtonsoft.Json;
using Strada.Api;

namespace Strada.Net461App
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var cloudServiceCredentials =
                JsonConvert.DeserializeObject<CloudServiceCredentials>(Resources.CloudServiceCredentials);

            var clientConfigSettings =
                JsonConvert.DeserializeObject<ClientConfigSettings>(Resources.ClientConfigSettings);

            Agent.Instance.Start(cloudServiceCredentials, clientConfigSettings);
        }
    }
}
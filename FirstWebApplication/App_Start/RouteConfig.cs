using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FirstWebApplication
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "UserForgotPassword",
                url: "User/ForgotPassword/{activationCode}",
                defaults: new { controller = "User", action = "ForgotPassword", activationCode = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "User",
                url: "User/VarifyAccount/{activationCode}",
                defaults: new { controller = "User", action = "VarifyAccount", activationCode = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
            
        }
    }
}

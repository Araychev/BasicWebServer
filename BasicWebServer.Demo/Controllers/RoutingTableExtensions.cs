﻿using BasicWebServer.Server.Controllers;
using BasicWebServer.Server.HTTP;
using BasicWebServer.Server.Routing;

namespace BasicWebServer.Demo.Controllers
{
    public static class RoutingTableExtensions
    {
        public static IRoutingTable MapGet<TController>(
            this IRoutingTable routingTable,
            string path,
            Func<TController, Response> controllerFunction)
            where TController : Controller 
            => routingTable.MapPost(path,
            request => controllerFunction(
                CreateController<TController>(request)));

        private static TController CreateController<TController>(Request request)
            =>
            (TController)Activator
                .CreateInstance(typeof(TController), new[] { request });

    }
}

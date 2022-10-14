using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OpenTelemetryDemo
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configuration.Formatters.Remove(
                GlobalConfiguration.Configuration.Formatters.XmlFormatter);
            
            Telemetry.Initialize();
            GlobalConfiguration.Configure(Register);


        }

        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
        }

        protected void Application_End()
        {
            Telemetry.Dispose();
        }
    }

    public static class Telemetry 
    {
        private const string serviceName = "Systematic.Demo.Service";
        private const string serviceVersion = "1.0.0";

        private static TracerProvider tracerProvider;
        public static ActivitySource ActivitySource { get; } = new ActivitySource(serviceName, serviceVersion);

        public static void Initialize()
        {
            tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddAspNetInstrumentation()
                .AddSource(serviceName)
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion))
                .AddOtlpExporter(options => {
                    options.ExportProcessorType = ExportProcessorType.Simple;
                    options.Endpoint = new Uri("http://sl-clud-048:4317");
                    options.Protocol = OtlpExportProtocol.Grpc;
                })
                .Build();
        }

        public static void Dispose()
        {
            tracerProvider?.Dispose();
        }
    }
}

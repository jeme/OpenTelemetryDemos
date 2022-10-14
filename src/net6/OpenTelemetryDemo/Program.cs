using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string serviceName = "Systematic.Demo.Service";
string serviceVersion = "1.0.0";

builder.Services.AddOpenTelemetryTracing(builder => {
    builder
        .AddSource(serviceName)
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion))
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddSqlClientInstrumentation()
        .AddOtlpExporter(options => {
            options.ExportProcessorType = ExportProcessorType.Simple;
            options.Endpoint = new Uri("http://sl-clud-048:4317");
            options.Protocol = OtlpExportProtocol.Grpc;
        });
});

ActivitySource activitySource = new ActivitySource(serviceName, serviceVersion);

WebApplication app = builder.Build();
app.MapGet("/api/values", async () =>
{
    await Task.Delay(200);
    using (Activity? activity = activitySource.StartActivity("SomeActivity"))
    {
        activity?.SetTag("mytag", "tag-value");
        await Task.Delay(200);
    }
    return new [] { "value1", "value2" };;
});
app.Run();


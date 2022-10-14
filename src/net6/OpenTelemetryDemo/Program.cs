using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;
using OpenTelemetry.Exporter;
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
builder.Services.AddSingleton<ActivitySource>(new ActivitySource(serviceName, serviceVersion));
builder.Services.AddSingleton<Dummy>();

WebApplication app = builder.Build();
app.MapGet("/api/values", async ([FromServices]ActivitySource activitySource, [FromServices]Dummy dummy) =>
{
    await Task.Delay(200);
    using (Activity? activity = activitySource.StartActivity("SomeActivity"))
    {
        activity?.SetTag("mytag", "tag-value");
        await Task.Delay(200);
        await dummy.DoDumbStuff();
    }
    return new [] { "value1", "value2" };
});
app.Run();


public class Dummy
{
    private readonly ActivitySource activitySource;

    public Dummy(ActivitySource activitySource)
    {
        this.activitySource = activitySource;
    }

    public async Task DoDumbStuff()
    {
        using (Activity? activity = activitySource.StartActivity())
        {
            activity?.SetTag("mytag", "tag-value");
            await Task.Delay(100);
        }
    }
}
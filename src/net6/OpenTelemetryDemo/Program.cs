WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();
app.MapGet("/api/values", () =>
{
    return new [] { "value1", "value2" };;
});
app.Run();
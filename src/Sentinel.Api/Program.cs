using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddCors();
// Dummy-service ile HTTP haberleşmesi için HttpClient eklendi.
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseCors(p => p.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(_ => true).AllowCredentials());
app.UseStaticFiles();
app.UseRouting();

app.MapHub<HealthHub>("/healthHub");

// Worker'ın sonuçları merkeze bildireceği endpoint
app.MapPost("/api/report", async (HealthReportDto report, IHubContext<HealthHub> hubContext) =>
{
    await hubContext.Clients.All.SendAsync("ReceiveHealthUpdate", report);
    return Results.Ok();
});

app.MapGet("/", async context =>
{
    context.Response.Redirect("/index.html");
    await Task.CompletedTask;
});

// Admin proxy endpoint'leri (CORS sorununu tamamen ortadan kaldırır)
app.MapPost("/api/admin/toggle-fault", async (IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient();
    var response = await client.PostAsync("http://dummy-service:8080/admin/toggle-fault", null);
    var content = await response.Content.ReadAsStringAsync();
    return Results.Content(content, "application/json");
});

app.MapPost("/api/admin/toggle-slow", async (IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient();
    var response = await client.PostAsync("http://dummy-service:8080/admin/toggle-slow", null);
    var content = await response.Content.ReadAsStringAsync();
    return Results.Content(content, "application/json");
});

app.Run("http://+:8080");

// Worker'dan gelen yeni 'ConsecutiveFailures' alanını içeren DTO
public record HealthReportDto(
    string ServiceName, 
    string Status, 
    string Latency, 
    int ConsecutiveFailures, 
    string Timestamp
);

public class HealthHub : Hub { }
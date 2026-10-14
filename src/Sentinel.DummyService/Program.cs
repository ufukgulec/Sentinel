var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Servis içi durum yönetimi (State)
var state = new ServiceState();
var rng = new Random();

// Sağlık kontrolü endpoint'i
app.MapGet("/health-check", async (HttpContext context) =>
{
    state.TotalRequests++;

    // Eğer manuel olarak arıza modu açıldıysa direkt 503 döndür
    if (state.IsForceDown)
    {
        state.FailedRequests++;
        return Results.StatusCode(503);
    }

    // Yapay gecikme (Normal veya yavaş mod)
    int delay = state.IsSlowMode ? rng.Next(1000, 3000) : rng.Next(50, 200);
    await Task.Delay(delay);

    // Rastgele hata üretimi (%30 ihtimalle)
    if (rng.Next(0, 10) > 6)
    {
        state.FailedRequests++;
        return Results.StatusCode(503);
    }

    state.SuccessfulRequests++;
    return Results.Ok(new 
    { 
        status = "Healthy", 
        latencyMs = delay,
        uptimeMinutes = (DateTime.UtcNow - state.StartTime).TotalMinutes,
        stats = new { state.TotalRequests, state.SuccessfulRequests, state.FailedRequests },
        timestamp = DateTime.UtcNow 
    });
});

// 1. Manuel olarak servisi patlatma / kurtarma endpoint'i
app.MapPost("/admin/toggle-fault", (bool? forceDown) =>
{
    state.IsForceDown = forceDown ?? !state.IsForceDown;
    return Results.Ok(new { message = $"Arıza modu: {state.IsForceDown}" });
});

// 2. Servisi yavaşlatma (Latency Spike) endpoint'i
app.MapPost("/admin/toggle-slow", (bool? slowMode) =>
{
    state.IsSlowMode = slowMode ?? !state.IsSlowMode;
    return Results.Ok(new { message = $"Yavaş mod: {state.IsSlowMode}" });
});

// Servis durumunu gösteren bilgi ekranı
app.MapGet("/admin/status", () => Results.Ok(state));

app.Run("http://+:8080");

// Durum sınıfı
public class ServiceState
{
    public DateTime StartTime { get; } = DateTime.UtcNow;
    public bool IsForceDown { get; set; } = false;
    public bool IsSlowMode { get; set; } = false;
    public long TotalRequests { get; set; }
    public long SuccessfulRequests { get; set; }
    public long FailedRequests { get; set; }
}
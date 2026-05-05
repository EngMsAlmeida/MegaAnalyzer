using MegaAnalyzer.Services;
using MegaAnalyzer.Components;
using MegaAnalyzer.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Banco de dados SQLite
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite("Data Source=megaanalyzer.db"));

// Serviços
builder.Services.AddScoped<ScrapingService>();

var app = builder.Build();

// Garante que o banco é criado com as migrations ao iniciar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var context = db.CreateDbContext();
    context.Database.EnsureCreated();
}

// Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
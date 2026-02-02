    using HSP;
using HSP.Data;
using HSP.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// EF Core DbContextFactory for Blazor Server
builder.Services.AddDbContextFactory<HspDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("HspDatabase"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// Authentication and Authorization
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationService>();

var app = builder.Build();

// Initialize database and seed default data
try
{
    using var scope = app.Services.CreateScope();
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<HspDbContext>>();
    await using var db = await dbFactory.CreateDbContextAsync();
    
    // Ensure database is created and apply migrations
    await db.Database.MigrateAsync();
    app.Logger.LogInformation("✅ Database migrations applied successfully");
    
    // Seed default administrator account
    await DbSeeder.SeedDefaultAdminAsync(db);
    
    app.Logger.LogInformation("✅ Database initialized successfully");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "❌ Database initialization failed: {Message}", ex.Message);
}

// HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Map Razor Components
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

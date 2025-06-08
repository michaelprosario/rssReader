using AppCore.Models.Feeds;
using AppCore.Models;
using AppCore.Repositories;
using AppCore.Services.Feeds;
using AppCore.Validators;
using AppInfra.Data;
using AppInfra.Repositories;
using BlogReaderApp.Components;
using BlogReaderApp.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfCoreRepository<>));

// Register validators
builder.Services.AddScoped<IValidator<Feed>, FeedValidator>();

// Register services
builder.Services.AddScoped<IFeedService, FeedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

// Create and seed database
app.CreateAndSeedDatabase();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

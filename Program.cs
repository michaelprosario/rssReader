using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using rssReader.Data;
using rssReader.Services;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// Register HttpClient
builder.Services.AddHttpClient();
builder.Services.AddSingleton<HttpClient>();

// Register our custom services
builder.Services.AddSingleton<IDataStorageService, JsonFileStorageService>();
builder.Services.AddSingleton<ISettingsService, SettingsService>();
builder.Services.AddSingleton<IFeedService, FeedService>();
builder.Services.AddSingleton<IArticleService, ArticleService>();
builder.Services.AddSingleton<ITagService, TagService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

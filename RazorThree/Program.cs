using Grpc.Net.Client;
using RazorThree;
using RazorThree.Protos;
using Grpc.Net.Client.Web;
using Grpc.Core;
using Grpc.Net;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Build a config object, using env vars and JSON providers.
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var url = config.GetRequiredSection("url").Get<string>();

builder.Services.AddGrpcClient<Recipe.RecipeClient>(client =>
{
    client.Address = new Uri(url);
}).ConfigurePrimaryHttpMessageHandler(
        () => new GrpcWebHandler(new HttpClientHandler()));


builder.Services.AddGrpcClient<Category.CategoryClient>(client =>
{
    client.Address = new Uri(url);
}).ConfigurePrimaryHttpMessageHandler(
        () => new GrpcWebHandler(new HttpClientHandler()));

// Get values from the config given their key and add it to base address of the client
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

app.UseAuthorization();

app.MapRazorPages();

app.Run();

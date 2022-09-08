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

builder.Services.AddGrpcClient<Recipe.RecipeClient>(o =>
{
    o.Address = new Uri(builder.Configuration["url"]);
}).ConfigurePrimaryHttpMessageHandler(
        () => new GrpcWebHandler(new HttpClientHandler()));

builder.Services.AddGrpcClient<Category.CategoryClient>(o =>
{
    o.Address = new Uri(builder.Configuration["url"]);
}).ConfigurePrimaryHttpMessageHandler(
        () => new GrpcWebHandler(new HttpClientHandler()));

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
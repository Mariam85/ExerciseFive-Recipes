using ServerFive.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();
builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader()
           .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
}));

var app = builder.Build();

app.UseCors();
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

// Configure the HTTP request pipeline.
app.MapGrpcService<RecipeService>().EnableGrpcWeb().RequireCors("AllowAll");
app.MapGrpcService<CategoryService>().EnableGrpcWeb().RequireCors("AllowAll");
app.MapGet("/", () => "This server contains a gRPCWeb service");

app.Run();

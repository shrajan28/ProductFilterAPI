using ProductFilterAPI.Services;
using System.Text.Json.Serialization;

using ProductFilterAPI.Services;
using ProductFilterAPI.Models;
using ProductFilterAPI;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
	options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add services to the container.
builder.Services.AddHttpClient<ProductService>();
builder.Services.AddMvc();
var app = builder.Build();

app.UseAuthorization();
app.MapControllers();




app.UseSwagger();
app.UseSwaggerUI();

app.Run();




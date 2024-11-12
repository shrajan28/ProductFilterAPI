using ProductFilterAPI.Services;
using System.Text.Json.Serialization;

using ProductFilterAPI.Services;
using ProductFilterAPI.Models;
using ProductFilterAPI;
using Microsoft.OpenApi.Models;
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
builder.Services.AddHttpClient<IProductService, ProductService>();// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


// Swagger configuration with Authorization
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProductFilterAPI", Version = "v1" });

	// Define the security scheme (either Basic or Bearer)
	c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "basic",
		In = ParameterLocation.Header,
		Description = "Basic Authorization header using the Basic scheme. Example: \"Authorization: Basic {token}\""
	});

	c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
	});

	// Apply security scheme globally to all controllers
	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "basic"  // Change to "bearer" if using JWT
                }
			},
			new string[] { }
		}
	});
});

var app = builder.Build();

// Enable middleware to serve Swagger
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductFilterAPI v1"));
}

// Configure the app pipeline
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseMiddleware<BasicAuthMiddleware>();


app.UseSwagger();
app.UseSwaggerUI();

app.Run();




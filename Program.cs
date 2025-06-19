using System.Text;
using System.Text.Json.Serialization;
using DotNetEnv;
using InnerBlend.API.Data;
using InnerBlend.API.Models;
using InnerBlend.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Configuration["ConnectionStrings:DefaultConnection"] = $"Host={Env.GetString("DB_HOST")};Port={Env.GetString("DB_PORT")};Database={Env.GetString("DB_NAME")};Username={Env.GetString("DB_USER")};Password={Env.GetString("DB_PASSWORD")}";
builder.Configuration["Jwt:Issuer"] = Env.GetString("JWT_ISSUER");
builder.Configuration["Jwt:Audience"] = Env.GetString("JWT_AUDIENCE");
builder.Configuration["Jwt:Key"] = Env.GetString("JWT_KEY");
builder.Configuration["Jwt:ExpiresInMinutes"] = Env.GetString("JWT_EXPIRES_IN_MINUTES");

builder.Configuration["Kestrel:Endpoints:Http:Url"] = Env.GetString("KESTREL_URL");

builder.Configuration["AzureBlobStorage:ConnectionString"] = Env.GetString("AZURE_STORAGE_CONNECTION_STRING");
builder.Configuration["AzureBlobStorage:ContainerName"] = Env.GetString("AZURE_BLOB_CONTAINER_NAME");	

builder.Services.AddControllersWithViews();
builder.Services.AddControllers().AddJsonOptions(options =>
{
	options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});

// Add DbContext and specify PostgreSQL connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Configuration.AddEnvironmentVariables();

// Add Identity services
builder
	.Services.AddIdentity<User, IdentityRole>(options =>
	{
		options.Password.RequireDigit = true;
		options.Password.RequireLowercase = true;
		options.Password.RequireNonAlphanumeric = false;
		options.Password.RequireUppercase = true;
		options.Password.RequiredLength = 8;
		options.Password.RequiredUniqueChars = 1;
	})
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddDefaultTokenProviders();

// Configure JWT authentication
builder
	.Services.AddAuthentication(options =>
	{
		options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
	})
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters =
			new Microsoft.IdentityModel.Tokens.TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = builder.Configuration["Jwt:Issuer"],
				ValidAudience = builder.Configuration["Jwt:Audience"],
				IssuerSigningKey = new SymmetricSecurityKey(
					Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty)
				),
				ClockSkew = TimeSpan.Zero
			};
	});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
	options.AddPolicy(
		"AllowAllOrigins",
		builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
	);
});
builder.Services.AddSingleton<BlobStorageServices>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");

app.UseAuthentication();
app.UseAuthorization();

// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };

// app.MapGet("/weatherforecast", () =>
// {
//     var forecast =  Enumerable.Range(1, 5).Select(index =>
//         new WeatherForecast
//         (
//             DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//             Random.Shared.Next(-20, 55),
//             summaries[Random.Shared.Next(summaries.Length)]
//         ))
//         .ToArray();
//     return forecast;
// })
// .WithName("GetWeatherForecast");

// Ensure the database is created and migrate any pending migrations
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	var dbContext = services.GetRequiredService<ApplicationDbContext>();
	dbContext.Database.Migrate();
}


app.MapControllers();

// Console.WriteLine(KeyGenerator.GenerateKey());

app.Run();

// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }

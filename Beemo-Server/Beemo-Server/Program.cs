using Beemo_Server.Data.Context;
using Beemo_Server.Dependencies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var allowBeemoOrigins = "_allowBeemoOrigins";

// Add services to the container.
builder.Services.AddControllers();

// Enable CORS for Beemo-Client
builder.Services.AddCors(options =>
{
    options.AddPolicy(allowBeemoOrigins, policy =>
    {
        // Change for Production
        policy.WithOrigins("http://localhost", "http://localhost:3000", "http://localhost:3001")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Set up Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new string[] {}
        }
    });
});

// Add authentication services
var secretKey = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("BeemoJwtKey"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Environment.GetEnvironmentVariable("BeemoJwtIssuer"),
            ValidAudience = Environment.GetEnvironmentVariable("BeemoJwtAudience"),
            IssuerSigningKey = new SymmetricSecurityKey(secretKey)
        };
    });

// Add database context
builder.Services.AddDbContextFactory<BeemoContext>(options =>
{
    // TODO: Swap to environment variable for production
    options.UseSqlServer(builder.Configuration.GetConnectionString("BeemoDbContext"));
});

// Dependency injection registry
builder.Services.RegisterServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors(allowBeemoOrigins);

app.MapControllers();

app.Run();

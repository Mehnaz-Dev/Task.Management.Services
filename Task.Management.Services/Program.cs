using CRM.Equitec.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Task.Management.Services;
using Task.Management.Services.Repositories;

Assembly assembly = Assembly.GetExecutingAssembly();
FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();

builder.Services.AddCors(config => config.AddDefaultPolicy(options =>
{
    options.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod();
}));

builder.Services.AddControllers();
builder.Services.AddMvc().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<AuthenticateRepository>();
builder.Services.AddDbContext<TaskManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.IncludeErrorDetails = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
    options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(5);
});

builder.Services.AddSwaggerGen(config =>
{
    config.SwaggerDoc(fileVersionInfo.FileVersion, new OpenApiInfo
    {
        Version = fileVersionInfo.ProductVersion,
        Title = "Task Management API",
        Description = "This is a private API for task management."
    });

    config.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Description = $@"Standard Authorization header using the {JwtBearerDefaults.AuthenticationScheme} scheme. Example: ""Bearer {{token}}""",
        In = ParameterLocation.Header,
        Name = HeaderNames.Authorization,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT"
    });
    config.OperationFilter<AuthHeaderOperationFilter>();
    config.DocumentFilter<CustomDocumentFilter>();
    config.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            new string[] {}
        }
    });

    config.EnableAnnotations();
});

var app = builder.Build();

app.MapHealthChecks("/HealthCheck");
app.UseSwagger();
app.UseSwaggerUI(config =>
{
    config.SwaggerEndpoint($"./{fileVersionInfo.FileVersion}/swagger.json", $"Version — {fileVersionInfo.FileVersion}");
    config.EnableFilter();
});

app.UseForwardedHeaders();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

using AspNetCoreRateLimit;
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
using Task.Management.Services.Models;
using Task.Management.Services.Services;


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
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("EndPointRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
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
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddSwaggerGen(config =>
{
    config.SwaggerDoc(fileVersionInfo.FileVersion, new OpenApiInfo
    {
        Version = fileVersionInfo.ProductVersion,
        Title = "CRM Mobile Data Access API",
        Description = $"This is Equitec restful **private API** catering CRM data",
        Contact = new OpenApiContact
        {
            Name = "Equitec Software Technology",
            Email = "support@equitec.in",
            Url = new Uri("https://equitec.in/#contact")
        }
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
app.UseIpRateLimiting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
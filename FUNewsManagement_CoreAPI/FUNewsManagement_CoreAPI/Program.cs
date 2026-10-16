using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using HoangNV_SE1930_A01_BE.DataAccess.Context;
using HoangNV_SE1930_A01_BE.DataAccess.Repositories;
using HoangNV_SE1930_A01_BE.Hubs;

var builder = WebApplication.CreateBuilder(args);

// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Connection string & DbContext
var connectionString = builder.Configuration.GetConnectionString("MyCnn");
builder.Services.AddDbContext<FunewsManagementContext>(opt => 
    opt.UseSqlServer(connectionString));

// Repositories & Services DI
builder.Services.AddScoped<ISystemAccountRepository, SystemAccountRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<INewsArticleRepository, NewsArticleRepository>();
builder.Services.AddScoped<HoangNV_SE1930_A01_BE.BusinessLogic.Services.IAuthService, HoangNV_SE1930_A01_BE.BusinessLogic.Services.AuthService>();
builder.Services.AddScoped<HoangNV_SE1930_A01_BE.BusinessLogic.Services.IAccountService, HoangNV_SE1930_A01_BE.BusinessLogic.Services.AccountService>();
builder.Services.AddScoped<HoangNV_SE1930_A01_BE.BusinessLogic.Services.ICategoryService, HoangNV_SE1930_A01_BE.BusinessLogic.Services.CategoryService>();
builder.Services.AddScoped<HoangNV_SE1930_A01_BE.BusinessLogic.Services.ITagService, HoangNV_SE1930_A01_BE.BusinessLogic.Services.TagService>();
builder.Services.AddScoped<HoangNV_SE1930_A01_BE.BusinessLogic.Services.INewsArticleService, HoangNV_SE1930_A01_BE.BusinessLogic.Services.NewsArticleService>();
builder.Services.AddScoped<HoangNV_SE1930_A01_BE.BusinessLogic.Services.IReportService, HoangNV_SE1930_A01_BE.BusinessLogic.Services.ReportService>();

// SignalR
builder.Services.AddSignalR();

// Controllers with JSON Options & OData
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    })
    .AddOData(opt => opt
        .Select()
        .Filter()
        .OrderBy()
        .Expand()
        .Count()
        .SetMaxTop(100));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "FUNewsManagementSystem_SuperSecretKey_2024_SecurityToken_@123456";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "FUNewsManagementSystem_BE";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "FUNewsManagementSystem_FE";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };

    // Configure Token extraction for SignalR Hub
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Swagger with JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FUNewsManagementSystem Core API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5237", "https://localhost:5237")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Ensure wwwroot/uploads folder exists for static file serving
var wwwrootFolder = app.Environment.WebRootPath ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
var uploadsFolder = Path.Combine(wwwrootFolder, "uploads");
if (!Directory.Exists(uploadsFolder))
{
    Directory.CreateDirectory(uploadsFolder);
}

app.UseStaticFiles();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();

using Microsoft.OpenApi.Models;
using FUNewsManagement_AIAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Singleton Learning Cache to persist selection statistics in memory across requests
builder.Services.AddSingleton<ILearningCacheService, LearningCacheService>();

// HttpClient & AiTagSuggestionService
builder.Services.AddHttpClient<IAiTagSuggestionService, AiTagSuggestionService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FUNewsManagement AI API", Version = "v1" });
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowFrontend");
app.MapControllers();

app.Run();

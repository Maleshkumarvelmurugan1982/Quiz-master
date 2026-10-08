using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuizApp.Data;
using QuizApp.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// DATABASE
// ============================================================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

// ============================================================
// JWT AUTHENTICATION
// ============================================================

var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? "QuizAppSuperSecretKey2024!@#$";

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"]
                    ?? "QuizApp",

                ValidAudience =
                    builder.Configuration["Jwt:Audience"]
                    ?? "QuizAppUsers",

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtKey))
            };
    });

builder.Services.AddAuthorization();

// ============================================================
// APPLICATION SERVICES
// ============================================================

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IQuizService, QuizService>();

builder.Services.AddScoped<IQuestionService, QuestionService>();

builder.Services.AddScoped<IAttemptService, AttemptService>();

builder.Services.AddScoped<
    IQuizGeneratorService,
    QuizGeneratorService>();

// ============================================================
// OPENROUTER HTTP CLIENT
// ============================================================
//
// QuizGeneratorService calls:
// POST https://openrouter.ai/api/v1/chat/completions
//
// The timeout is handled inside QuizGeneratorService so that
// each AI request has its own cancellation/timeout handling.
// ============================================================

builder.Services.AddHttpClient(
    "openrouter",
    client =>
    {
        client.BaseAddress =
            new Uri(
                "https://openrouter.ai/");

        client.Timeout =
            Timeout.InfiniteTimeSpan;

        client.DefaultRequestHeaders
            .UserAgent.ParseAdd(
                "QuizApp/1.0");
    });

// ============================================================
// CONTROLLERS + SWAGGER
// ============================================================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Quiz Application API",
            Version = "v1"
        });

    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description =
                "JWT Authorization. Enter: Bearer {token}",

            Name = "Authorization",

            In = ParameterLocation.Header,

            Type = SecuritySchemeType.ApiKey,

            Scheme = "Bearer"
        });

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,

                            Id = "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        });
});

// ============================================================
// CORS
// ============================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// ============================================================
// BUILD APP
// ============================================================

var app = builder.Build();

// ============================================================
// SWAGGER
// ============================================================

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "Quiz App API v1");

    c.RoutePrefix = "swagger";
});

// ============================================================
// FRONTEND
// ============================================================

app.UseDefaultFiles();

app.UseStaticFiles();

// ============================================================
// MIDDLEWARE
// ============================================================

app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();

// ============================================================
// API CONTROLLERS
// ============================================================

app.MapControllers();

// ============================================================
// SPA FALLBACK
// ============================================================

app.MapFallbackToFile(
    "index.html");

// ============================================================
// RUN
// ============================================================

app.Run();

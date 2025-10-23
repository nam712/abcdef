using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using YourShopManagement.API.Data;
using YourShopManagement.API.Helpers;
using YourShopManagement.API.Services;

var builder = WebApplication.CreateBuilder(args);

// ==================== 1. ADD SERVICES ====================

// Add Controllers
builder.Services.AddControllers();

// Add DbContext v?i PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

// ==================== 2. JWT AUTHENTICATION ====================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
        ClockSkew = TimeSpan.Zero // Lo?i b? delay m?c ??nh 5 ph�t
    };

    // X? l� c�c s? ki?n JWT
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                message = "Token kh�ng h?p l? ho?c ?� h?t h?n"
            });
            return context.Response.WriteAsync(result);
        }
    };
});

builder.Services.AddAuthorization();

// ==================== 3. CORS ====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

});

// ==================== 4. DEPENDENCY INJECTION ====================

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Helpers
builder.Services.AddScoped<JwtHelper>();

// Repositories
builder.Services.AddScoped<YourShopManagement.API.Repositories.ISupplierRepository, YourShopManagement.API.Repositories.SupplierRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();

// ==================== 5. SWAGGER ====================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Shop Management API",
        Version = "v1",
        Description = "API qu?n l� c?a h�ng v?i JWT Authentication",
        Contact = new OpenApiContact
        {
            Name = "Your Name",
            Email = "your.email@example.com"
        }
    });

    // C?u h�nh JWT trong Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header s? d?ng Bearer scheme. \r\n\r\n" +
                      "Nh?p 'Bearer' [space] v� sau ?� nh?p token c?a b?n.\r\n\r\n" +
                      "V� d?: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
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

// ==================== BUILD APP ====================
var app = builder.Build();

// ==================== 6. MIDDLEWARE PIPELINE ====================

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shop Management API v1");
        c.RoutePrefix = "swagger"; // Swagger UI t?i /swagger
    });
}

app.UseHttpsRedirection();

// CORS - ph?i ??t tr??c Authentication
app.UseCors("AllowAll"); // Ho?c "AllowSpecificOrigins"

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ==================== 7. AUTO MIGRATE DATABASE (Optional) ====================
// T? ??ng apply migrations khi ch?y app (ch? d�ng cho Development)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            // Ki?m tra k?t n?i
            await dbContext.Database.CanConnectAsync();
            Console.WriteLine("? K?t n?i database th�nh c�ng!");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"? L?i k?t n?i database: {ex.Message}");
        }
    }
}

app.Run();
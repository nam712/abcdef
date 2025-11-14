using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Services;
using System.Text;
using YourShopManagement.API.Data;
using YourShopManagement.API.Helpers;
using YourShopManagement.API.Services;
using Backend.Models.Momo;
using Backend.Services.Momo;
using Backend.Repositories;
using Backend.Services;
using YourShopManagement.API.Repositories;
using Backend.Mappings;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// ==================== 1. ADD SERVICES ====================
//Momo API Payment
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();

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
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });

});

// ==================== 4. DEPENDENCY INJECTION ====================

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(typeof(EntityMappingProfile));
// Helpers
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddHttpContextAccessor();

// Repositories
builder.Services.AddScoped<YourShopManagement.API.Repositories.ISupplierRepository, YourShopManagement.API.Repositories.SupplierRepository>();
builder.Services.AddScoped<YourShopManagement.API.Repositories.ICustomerRepository, YourShopManagement.API.Repositories.CustomerRepository>();
builder.Services.AddScoped<YourShopManagement.API.Repositories.IProductRepository, YourShopManagement.API.Repositories.ProductRepository>();
builder.Services.AddScoped<YourShopManagement.API.Repositories.IProductCategoryRepository, YourShopManagement.API.Repositories.ProductCategoryRepository>();
builder.Services.AddScoped<YourShopManagement.API.Repositories.IPurchaseOrderRepository, YourShopManagement.API.Repositories.PurchaseOrderRepository>();


// Services
builder.Services.AddHttpClient<YourShopManagement.API.Services.SmsService.ISmsService, YourShopManagement.API.Services.SmsService.SmsService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();

// PurchaseOrder service registration (register interface -> implementation)
builder.Services.AddScoped<YourShopManagement.API.Services.IPurchaseOrderService, YourShopManagement.API.Services.PurchaseOrderService>();

// Đăng ký Invoice service (sửa lỗi: Unable to resolve IInvoiceService khi khởi tạo InvoicesController)
builder.Services.AddScoped<YourShopManagement.API.Services.Interfaces.IInvoiceService, YourShopManagement.API.Services.InvoiceService.InvoiceService>();

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();

// Shop
builder.Services.AddScoped<YourShopManagement.API.Repositories.ShopRepository.IShopRepository, YourShopManagement.API.Repositories.ShopRepository.ShopRepository>();
builder.Services.AddScoped<YourShopManagement.API.Services.ShopService.IShopService, YourShopManagement.API.Services.ShopService.ShopService>();

// Promotion
builder.Services.AddScoped<YourShopManagement.API.Repositories.PromotionRepository.IPromotionRepository, YourShopManagement.API.Repositories.PromotionRepository.PromotionRepository>();
builder.Services.AddScoped<YourShopManagement.API.Services.PromotionService.IPromotionService, YourShopManagement.API.Services.PromotionService.PromotionService>();

// ==================== 5. SWAGGER ====================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Shop Management API",
        Version = "v1",
        Description = "API quan ly cua hang v?i JWT Authentication",
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
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()     // Cho phép mọi domain
                  .AllowAnyHeader()     // Cho phép mọi header
                  .AllowAnyMethod();    // Cho phép mọi phương thức (GET, POST, PUT, DELETE...)
        });
});
// ==================== BUILD APP ====================
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");
// ⚠️ IMPORTANT: CORS phải đặt trước UseAuthentication
app.UseCors("AllowAngular");

// Tạm thời tắt HTTPS redirect để test
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles(); // Phục vụ wwwroot mặc định

// Đảm bảo thư mục uploads tồn tại
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// Cấu hình phục vụ thư mục uploads
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.MapControllers();


// Log để biết server đang chạy
Console.WriteLine("========================================");
Console.WriteLine("🚀 Backend is running!");
Console.WriteLine("📍 API Base URL: http://localhost:5001/api");
Console.WriteLine("📝 Swagger UI: http://localhost:5001/swagger");
Console.WriteLine("🔐 Auth Endpoint: http://localhost:5001/api/auth/register");
Console.WriteLine("========================================");

app.Run();
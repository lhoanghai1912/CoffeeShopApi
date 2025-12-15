using CoffeeShopApi.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;
using CoffeeShopApi.Services;
using CoffeeShopApi.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Ghi đè hành vi mặc định khi Validate lỗi
        options.InvalidModelStateResponseFactory = context =>
        {
            // 1. Lấy tất cả thông báo lỗi từ ModelState
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            // 2. Tạo response theo chuẩn ApiResponse của bạn
            // Dùng <object> vì lúc này không trả về data cụ thể nào
            var response = ApiResponse<object>.Fail("Dữ liệu đầu vào không hợp lệ", errors);

            // 3. Trả về BadRequest (400) kèm response đã bọc
            return new BadRequestObjectResult(response);
        };
    });
  

// 1. Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Authenticate JWT 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// Authorize 
builder.Services.AddSwaggerGen(c =>
{
  //  c.SwaggerDoc("v1", new OpenApiInfo { Title = "CoffeeShopApi", Version = "v1" });
    
    // Định nghĩa bảo mật Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập token theo định dạng: Bearer {token}",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

// Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký Services (Dependency Injection)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});


var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {

    });
    app.UseSwaggerUI();
    app.MapScalarApiReference(options =>
    {
        // Quan trọng: Chỉ định Scalar đọc file JSON do Swagger tạo ra
        options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
        options.WithTitle("Coffee Shop API");
        // options.WithTheme(ScalarTheme); // Chọn theme: Moon, Mars, DeepSpace...
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();

app.Run();
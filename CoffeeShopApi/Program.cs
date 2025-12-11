using CoffeeShopApi.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text;
using CoffeeShopApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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
app.UseAuthentication(); // <-- Thêm dòng này
app.UseAuthorization();
app.MapControllers();

app.Run();
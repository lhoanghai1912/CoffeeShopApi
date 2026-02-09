using CoffeeShopApi.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;
using CoffeeShopApi.Services;
using CoffeeShopApi.Repositories;
using CoffeeShopApi.Authorization;
using CoffeeShopApi.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {

        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .SelectMany(x => x.Value.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();

            var response = ApiResponse<object>.Fail(errors);

            return new BadRequestObjectResult(response);
        };
    })
    .AddJsonOptions(x => 
        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

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

// ✅ Authorization - Đăng ký Permission Handler
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddAuthorization(options =>
{
    // Tự động tạo policies cho các permissions
    var permissions = new[]
    {
        "product.view", "product.create", "product.update", "product.delete",
        "category.view", "category.create", "category.update", "category.delete",
        "order.view.own", "order.view.all", "order.create", "order.update.own", "order.update.all",
        "order.cancel.own", "order.cancel.all",
        "user.view.own", "user.view.all", "user.update.own", "user.update.all", "user.delete",
        "role.manage", "permission.assign"
    };

    foreach (var permission in permissions)
    {
        options.AddPolicy($"RequirePermission:{permission}", policy =>
            policy.Requirements.Add(new PermissionRequirement(permission)));
    }
});

// Authorize 
builder.Services.AddSwaggerGen(c =>
{
  //  c.SwaggerDoc("v1", new OpenApiInfo { Title = "CoffeeShopApi", Version = "v1" });
    
    // Định nghĩa bảo mật Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token vào bên dưới (không cần 'Bearer ' ở đầu)."
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

// Đăng ký Repositories (Dependency Injection)
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Đăng ký Services (Dependency Injection)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ICategoryservice, CategoriesService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IProductRequestService, ProductRequestService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();

// User address service
builder.Services.AddScoped<IUserAddressService, UserAddressService>();
// Voucher service
builder.Services.AddScoped<IVoucherService, VoucherService>();
// Email service and settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// File upload settings
builder.Services.Configure<CoffeeShopApi.Settings.FileUploadSettings>(builder.Configuration.GetSection("FileUploadSettings"));

// ✅ Background Service - Tự động cập nhật IsActive của vouchers
builder.Services.AddHostedService<VoucherStatusUpdateService>();


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
app.UseMiddleware<CoffeeShopApi.Middlewares.ErrorHandlingMiddleware>();
// app.UseMiddleware<CoffeeShopApi.Middlewares.RequestLoggingMiddleware>();
// app.UseMiddleware<CoffeeShopApi.Middlewares.AuthenticationInfoMiddleware>();
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();

// Initialize Database - Seed products with OptionGroups
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

    // ⚠️ CHÚ Ý: Không tự động migrate khi start app
    // Chỉ apply migrations thủ công bằng: dotnet ef database update
    // Lý do: Migration có thể xóa data nếu schema thay đổi lớn

    // Apply any pending migrations - CHỈ BẬT KHI CẦN
    // try
    // {
    //     context.Database.Migrate();
    // }
    // catch
    // {
    //     // Ignore migration errors in development here
    // }

    // Chỉ seed data nếu DB đã được setup
    try
    {
        await DbInitializer.InitializeAsync(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error initializing database: {ex.Message}");
    }
}

app.Run();

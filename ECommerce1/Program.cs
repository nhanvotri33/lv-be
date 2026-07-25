using ECommerce.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using ECommerce1.Services;

// Cấu hình hiển thị tiếng Việt có dấu trên màn hình Console của Windows
Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình DbContext & Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ECommerce API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập Token theo định dạng: Bearer {your_token}",
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
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductVariantService, ProductVariantService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ECommerce1.Services.Payment.IPaymentProvider, ECommerce1.Services.Payment.StripePaymentProvider>();
builder.Services.AddScoped<ECommerce1.Services.Payment.IPaymentProvider, ECommerce1.Services.Payment.MomoPaymentProvider>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IAhamoveService, AhamoveService>();

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey)) jwtKey = "your_super_secret_key_make_it_long_enough";

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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    // Xử lý thông báo lỗi "Không được quyền" (Forbidden 403)
    options.Events = new JwtBearerEvents
    {
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json; charset=utf-8";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                message = "Bạn không có quyền thực hiện thao tác này. Chỉ dành cho Admin."
            });
            return context.Response.WriteAsync(result);
        },
        OnChallenge = context =>
        {
            // Bỏ qua nếu phản hồi đã được xử lý ở nơi khác
            context.HandleResponse();

            // Kiểm tra xem header đã gửi đi chưa để tránh lỗi ghi đè
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json; charset=utf-8";

                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    message = "Bạn cần đăng nhập để truy cập chức năng này."
                });

                return context.Response.WriteAsync(result);
            }

            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

app.UseMiddleware<ECommerce1.Middlewares.ExceptionHandlingMiddleware>();

// 3. Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles(); // Cho phép truy cập ảnh trong wwwroot/uploads

app.UseCors("AllowAll"); // Bật CORS trước Authentication

// Thứ tự này rất quan trọng: Authentication trước, Authorization sau
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public static class DataSeeder
{
    public static void SeedExampleData(ApplicationDbContext dbContext)
    {
        // 1. Seed OrderStatuses if empty
        if (!dbContext.OrderStatuses.Any())
        {
            dbContext.Database.OpenConnection();
            try
            {
                dbContext.Database.ExecuteSqlRaw(@"
                    SET IDENTITY_INSERT OrderStatuses ON;
                    INSERT INTO OrderStatuses (Id, Name, Description) VALUES (1, 'Pending', N'Chờ thanh toán / Chờ xác nhận');
                    INSERT INTO OrderStatuses (Id, Name, Description) VALUES (2, 'Processing', N'Đang xử lý / Đóng gói');
                    INSERT INTO OrderStatuses (Id, Name, Description) VALUES (3, 'Shipping', N'Đang vận chuyển');
                    INSERT INTO OrderStatuses (Id, Name, Description) VALUES (4, 'Completed', N'Đã giao hàng thành công');
                    INSERT INTO OrderStatuses (Id, Name, Description) VALUES (5, 'Cancelled', N'Đã hủy');
                    INSERT INTO OrderStatuses (Id, Name, Description) VALUES (6, 'Return_failed', N'Giao hàng thất bại / Hoàn hàng');
                    INSERT INTO OrderStatuses (Id, Name, Description) VALUES (7, 'Refunded', N'Đổi trả / Hoàn tiền');
                    SET IDENTITY_INSERT OrderStatuses OFF;
                ");
            }
            finally
            {
                dbContext.Database.CloseConnection();
            }
        }

        // 2. Seed Provinces if empty
        if (!dbContext.Provinces.Any())
        {
            var provinces = new List<Province>
            {
                new Province { Id = "79", Name = "Hồ Chí Minh", FullName = "Thành phố Hồ Chí Minh", CodeName = "ho_chi_minh" },
                new Province { Id = "01", Name = "Hà Nội", FullName = "Thành phố Hà Nội", CodeName = "ha_noi" },
                new Province { Id = "48", Name = "Đà Nẵng", FullName = "Thành phố Đà Nẵng", CodeName = "da_nang" }
            };
            dbContext.Provinces.AddRange(provinces);
            dbContext.SaveChanges();
        }

        // 3. Seed Wards if empty
        if (!dbContext.Wards.Any())
        {
            var wards = new List<Ward>
            {
                // HCM
                new Ward { Id = "26734", Name = "Bến Nghé", FullName = "Phường Bến Nghé", CodeName = "ben_nghe", ProvinceId = "79" },
                new Ward { Id = "26740", Name = "Đa Kao", FullName = "Phường Đa Kao", CodeName = "da_kao", ProvinceId = "79" },
                new Ward { Id = "26745", Name = "Tân Định", FullName = "Phường Tân Định", CodeName = "tan_dinh", ProvinceId = "79" },
                // HN
                new Ward { Id = "00001", Name = "Phúc Xá", FullName = "Phường Phúc Xá", CodeName = "phuc_xa", ProvinceId = "01" },
                new Ward { Id = "00004", Name = "Trúc Bạch", FullName = "Phường Trúc Bạch", CodeName = "truc_bach", ProvinceId = "01" },
                new Ward { Id = "00006", Name = "Vĩnh Phúc", FullName = "Phường Vĩnh Phúc", CodeName = "vinh_phuc", ProvinceId = "01" },
                // DN
                new Ward { Id = "20197", Name = "Hải Châu I", FullName = "Phường Hải Châu I", CodeName = "hai_chau_i", ProvinceId = "48" },
                new Ward { Id = "20200", Name = "Hải Châu II", FullName = "Phường Hải Châu II", CodeName = "hai_chau_ii", ProvinceId = "48" },
                new Ward { Id = "20203", Name = "Thạch Thang", FullName = "Phường Thạch Thang", CodeName = "thach_thang", ProvinceId = "48" }
            };
            dbContext.Wards.AddRange(wards);
            dbContext.SaveChanges();
        }
    }
}

//using ECommerce.Models;
//using Microsoft.EntityFrameworkCore;
//using System;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.AddScoped<TokenService>();

//var app = builder.Build();
//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthentication();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();

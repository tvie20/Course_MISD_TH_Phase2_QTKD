using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.Middleware;
using QTKD.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình DbContext với PostgreSQL
builder.Services.AddDbContext<QtkdDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// 2. Đăng ký Repository truy xuất dữ liệu
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IPriceListRepository, PriceListRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3. Sử dụng Middleware xử lý ngoại lệ
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QTKD.Api.Data;

namespace QTKD.Tests.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Xóa registration DbContext cũ
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<QtkdDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Thay bằng InMemory database (tên khác nhau mỗi lần để tránh dữ liệu bị chia sẻ)
            services.AddDbContext<QtkdDbContext>(options =>
                options.UseInMemoryDatabase("QTKDTestDb_" + Guid.NewGuid().ToString()));

            // Đảm bảo schema được tạo
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
            db.Database.EnsureCreated();
        });

        // Chạy ở môi trường Test để tránh các cấu hình Production
        builder.UseEnvironment("Test");
    }
}
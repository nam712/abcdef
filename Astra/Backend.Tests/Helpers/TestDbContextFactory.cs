using System;
using Microsoft.EntityFrameworkCore;
using YourShopManagement.API.Data;

namespace Backend.Tests
{
    // Factory để tạo InMemory ApplicationDbContext tái sử dụng trong unit tests
    public static class TestDbContextFactory
    {
        // Tạo context với tên DB (mặc định random => isolated per test)
        public static ApplicationDbContext Create(string? dbName = null)
        {
            var name = dbName ?? Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(name)
                .Options;
            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        // Tạo context và chạy seed action, rồi lưu
        public static ApplicationDbContext CreateWithSeed(Action<ApplicationDbContext> seed, string? dbName = null)
        {
            var ctx = Create(dbName);
            seed?.Invoke(ctx);
            ctx.SaveChanges();
            return ctx;
        }

        // Dọn dẹp DB sau test
        public static void Destroy(ApplicationDbContext context)
        {
            if (context == null) return;
            context.Database.EnsureDeleted();
            context.Dispose();
        }
    }
}

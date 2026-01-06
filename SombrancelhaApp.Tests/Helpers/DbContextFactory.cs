using Microsoft.EntityFrameworkCore;
using SombrancelhaApp.Api.Infrastructure.Data;

namespace SombrancelhaApp.Tests.Helpers;

public static class DbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}

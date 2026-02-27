using Microsoft.EntityFrameworkCore;

namespace NewsSite.Infrastructure.Data;

public class AppDbContextFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite("Data Source=newssite.db");

        return new AppDbContext(optionsBuilder.Options);
    }
}
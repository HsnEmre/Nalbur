using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nalbur.Infrastructure.Data;

public class NalburDbContextFactory : IDesignTimeDbContextFactory<NalburDbContext>
{
    public NalburDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NalburDbContext>();
        optionsBuilder.UseSqlServer("Server=DESKTOP-41V1OLM\\SQLEXPRESS;Database=NalburDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True");

        return new NalburDbContext(optionsBuilder.Options);
    }
}

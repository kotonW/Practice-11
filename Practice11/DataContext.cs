using Microsoft.EntityFrameworkCore;

namespace Practice11;

public class DataContext : DbContext
{
    public DbSet<Note> Notes => Set<Note>();
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=test.db");
        optionsBuilder.LogTo(Console.WriteLine);
        base.OnConfiguring(optionsBuilder);
    }
}
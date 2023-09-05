using Microsoft.EntityFrameworkCore;
namespace DataBaseWeb.Data;


public class DataBaseContext : DbContext
{

    public DataBaseContext (DbContextOptions<DataBaseContext> options)
        : base(options)
    {
    }

    public DbSet<DataBaseLib.DataBase> DataBase { get; set; } = default!;
}
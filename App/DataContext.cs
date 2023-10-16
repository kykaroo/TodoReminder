using App.Models;
using Microsoft.EntityFrameworkCore;

namespace App;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<TodoItem> Items { get; set; } = default!;
    public DbSet<User> Users { get; set; } = default!;
}
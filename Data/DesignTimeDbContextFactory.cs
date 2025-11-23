using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TodoApp.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TodoContext>
{
    public TodoContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TodoContext>();
        optionsBuilder.UseSqlite("Data Source=todo.db");

        return new TodoContext(optionsBuilder.Options);
    }
}
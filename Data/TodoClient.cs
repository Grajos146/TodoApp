using Microsoft.EntityFrameworkCore;

namespace TodoApp.Data;

public class TodoClient
{
    public static async Task MigrateDbAsync(IHost app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoContext>();
        await db.Database.MigrateAsync();
    }
}

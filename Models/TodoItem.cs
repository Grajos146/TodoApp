using System;

namespace TodoApp.Models;

public class TodoItem
{
    //unique identifier for the todo item
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsCompleted { get; set; }

    //user id to associate todo item with a user
    public string? UserId { get; set; }
}

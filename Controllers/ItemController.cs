using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    [Authorize]
    [Route("items")]
    public class ItemController : Controller
    {
        // GET: ItemController

        private readonly TodoContext _context;
        private readonly ILogger<ItemController> _logger;

        public ItemController(TodoContext context, ILogger<ItemController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Item
        [Route("")]
        public async Task<ActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var items = await _context.TodoItems.Where(u => u.UserId == userId).ToListAsync();
            if (items == null || items.Count == 0)
            {
                ViewBag.Message = "No todo items found.";
                return View(new List<TodoItem>());
            }
            return View(items);
        }

        // POST: Item

        [HttpGet("create")]
        public async Task<ActionResult> Create()
        {
            await Task.CompletedTask;
            return View();
        }

        [HttpPost]
        [Route("create")]
        public async Task<ActionResult> Create([Bind("Title", "Description")] TodoItem item)
        {
            _logger.LogInformation("Creating a new todo item: {Title}", item.Title);
            if (ModelState.IsValid)
            {
                try
                {
                    item.Id = Guid.NewGuid();
                    item.IsCompleted = false;
                    item.CreatedAt = DateTime.UtcNow;

                    item.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    _context.Add(item);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Todo item created successfully: {Title}", item.Title);

                    return RedirectToAction("Index", "Item");
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error occurred while creating todo item: {Title}",
                        item.Title
                    );
                    ModelState.AddModelError(
                        string.Empty,
                        "An error occurred while creating the todo item. Please try again."
                    );
                }
            }
            return View(item);
        }

        private bool TodoItemExists(Guid id)
        {
            return _context.TodoItems.Any(e => e.Id == id);
        }

        [HttpGet("edit/{id}")]
        public async Task<ActionResult> Edit(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var item = await _context.TodoItems.FirstOrDefaultAsync(t =>
                t.Id == id && t.UserId == userId
            );

            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        [HttpPost]
        [Route("edit/{id}")]
        public async Task<ActionResult> Edit(
            Guid id,
            [Bind("Id, Title, Description, IsCompleted")] TodoItem item
        )
        {
            _logger.LogInformation("Editing todo item: {Id}", item.Id);

            if (id != item.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    _logger.LogInformation(
                        "Fetching existing todo item for editing: {Id}",
                        item.Id
                    );
                    var existingItem = await _context.TodoItems.FirstOrDefaultAsync(u =>
                        u.Id == id && u.UserId == userId
                    );
                    if (existingItem == null)
                    {
                        _logger.LogWarning("Todo item not found for editing: {Id}", item.Id);
                        return NotFound();
                    }

                    existingItem.Title = item.Title;
                    existingItem.Description = item.Description;
                    existingItem.IsCompleted = item.IsCompleted;
                    existingItem.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Todo item edited successfully: {Id}", item.Id);

                    return RedirectToAction("Index", "Item");
                }
                catch (DbUpdateConcurrencyException)
                {
                    _logger.LogError("Concurrency error while editing todo item: {Id}", item.Id);
                    if (!TodoItemExists(item.Id))
                    {
                        _logger.LogWarning("Todo item not found during edit: {Id}", item.Id);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(
                            "Rethrowing concurrency exception for todo item: {Id}",
                            item.Id
                        );
                        throw;
                    }
                }
            }
            return View(item);
        }

        [HttpGet("delete/{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var item = await _context.TodoItems.FirstOrDefaultAsync(t =>
                t.Id == id && t.UserId == userId
            );
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [Route("delete/{id}")]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            _logger.LogInformation("Deleting todo item: {Id}", id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = await _context.TodoItems.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (item == null)
            {
                
                _logger.LogWarning("Todo item not found for deletion by user {UserId}: {Id}", userId, id);
                return NotFound();
            }

            try
            {
                _context.TodoItems.Remove(item);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Todo item deleted successfully: {Id}", id);

                return RedirectToAction("Index", "Item");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting todo item: {Id}", id);

                throw;
            }
        }


        // [HttpGet ("delete-all")]

        // public ActionResult DeleteAll()
        // {
        //     return View();
        // }
        
        // [HttpPost, ActionName("DeleteAllConfirmed")]
        // [Route("delete-all")]
        // public async Task<ActionResult> DeleteAllConfirmed()
        // {
        //     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //     var items = await _context.TodoItems.Where(u => u.UserId == userId).ToListAsync();

        //     if(items == null || !items.Any())
        //     {
        //         return RedirectToAction("Index", "Item");
        //     }
        //     else
        //     {
        //         _context.TodoItems.RemoveRange(items);

        //         await _context.SaveChangesAsync();
                
        //         return RedirectToAction("Index", "Item");
        //     }
        // }
    }
}

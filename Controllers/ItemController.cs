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

        public ItemController(TodoContext context)
        {
            _context = context;
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
            if (ModelState.IsValid)
            {
                item.Id = Guid.NewGuid();
                item.IsCompleted = false;
                item.CreatedAt = DateTime.UtcNow;

                item.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
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
            if (id != item.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    var existingItem = await _context.TodoItems.FirstOrDefaultAsync(u =>
                        u.Id == id && u.UserId == userId
                    );
                    if (existingItem == null)
                    {
                        return NotFound();
                    }

                    existingItem.Title = item.Title;
                    existingItem.Description = item.Description;
                    existingItem.IsCompleted = item.IsCompleted;
                    existingItem.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TodoItemExists(item.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
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

        [HttpPost]
        [Route("delete/{id}")]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            var item = await _context.TodoItems.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            _context.TodoItems.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}

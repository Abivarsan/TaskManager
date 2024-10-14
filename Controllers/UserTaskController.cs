// Controllers/UserTasksController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [Authorize]
    public class UserTasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserTasksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: UserTasks
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var userTasks = _context.UserTasks.Where(t => t.UserId == user.Id);
            return View(await userTasks.ToListAsync());
        }

        // GET: UserTasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var userTask = await _context.UserTasks
                .FirstOrDefaultAsync(m => m.UserTaskId == id && m.UserId == user.Id);
            if (userTask == null)
            {
                return NotFound();
            }

            return View(userTask);
        }

        // GET: UserTasks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserTasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description")] UserTask userTask)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                userTask.UserId = user.Id;
                _context.Add(userTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userTask);
        }

        // GET: UserTasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var userTask = await _context.UserTasks.FirstOrDefaultAsync(t => t.UserTaskId == id && t.UserId == user.Id);
            if (userTask == null)
            {
                return NotFound();
            }
            return View(userTask);
        }

        // POST: UserTasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserTaskId,Title,Description")] UserTask userTask)
        {
            if (id != userTask.UserTaskId)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var existingTask = await _context.UserTasks.AsNoTracking().FirstOrDefaultAsync(t => t.UserTaskId == id && t.UserId == user.Id);
            if (existingTask == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    userTask.UserId = user.Id;
                    _context.Update(userTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserTaskExists(userTask.UserTaskId, user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(userTask);
        }

        // GET: UserTasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var userTask = await _context.UserTasks
                .FirstOrDefaultAsync(m => m.UserTaskId == id && m.UserId == user.Id);
            if (userTask == null)
            {
                return NotFound();
            }

            return View(userTask);
        }

        // POST: UserTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var userTask = await _context.UserTasks.FirstOrDefaultAsync(t => t.UserTaskId == id && t.UserId == user.Id);
            if (userTask != null)
            {
                _context.UserTasks.Remove(userTask);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UserTaskExists(int id, string userId)
        {
            return _context.UserTasks.Any(e => e.UserTaskId == id && e.UserId == userId);
        }
    }
}

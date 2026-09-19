// Controllers/UserTasksController.cs
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
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
        private readonly IEmailSender _emailSender;
        private readonly ILogger<UserTasksController> _logger;

        public UserTasksController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            ILogger<UserTasksController> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        // GET: UserTasks
        public async Task<IActionResult> Index(string? category, string? priority, string? status, string? search)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var query = _context.UserTasks
                .Include(t => t.SubTasks)
                .Where(t => t.UserId == user.Id);

            // Filtering
            if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<TaskCategory>(category, true, out var catEnum))
            {
                query = query.Where(t => t.Category == catEnum);
            }

            if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<TaskPriority>(priority, true, out var prioEnum))
            {
                query = query.Where(t => t.Priority == prioEnum);
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Models.TaskStatus>(status, true, out var statEnum))
            {
                query = query.Where(t => t.Status == statEnum);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(t => t.Title.ToLower().Contains(lower) || (t.Description != null && t.Description.ToLower().Contains(lower)));
            }

            var tasks = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

            // Calculate overall analytics metrics across all user tasks
            var allUserTasks = await _context.UserTasks
                .Include(t => t.SubTasks)
                .Where(t => t.UserId == user.Id)
                .ToListAsync();

            var totalTasks = allUserTasks.Count;
            var completedTasks = allUserTasks.Count(t => t.Status == Models.TaskStatus.Completed);
            var inProgressTasks = allUserTasks.Count(t => t.Status == Models.TaskStatus.InProgress);
            var todoTasks = allUserTasks.Count(t => t.Status == Models.TaskStatus.Todo);
            var overdueTasks = allUserTasks.Count(t => t.DueDate.HasValue && t.DueDate.Value.Date < DateTime.Today && t.Status != Models.TaskStatus.Completed);
            var completionRate = totalTasks > 0 ? (int)Math.Round((double)completedTasks / totalTasks * 100) : 0;

            ViewBag.TotalTasks = totalTasks;
            ViewBag.CompletedTasks = completedTasks;
            ViewBag.InProgressTasks = inProgressTasks;
            ViewBag.TodoTasks = todoTasks;
            ViewBag.OverdueTasks = overdueTasks;
            ViewBag.CompletionRate = completionRate;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedPriority = priority;
            ViewBag.SelectedStatus = status;
            ViewBag.SearchTerm = search;

            return View(tasks);
        }

        // GET: UserTasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var userTask = await _context.UserTasks
                .Include(t => t.SubTasks.OrderBy(s => s.CreatedAt))
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
            return View(new UserTask
            {
                Priority = TaskPriority.Medium,
                Status = Models.TaskStatus.Todo,
                Category = TaskCategory.General,
                DueDate = DateTime.Today.AddDays(3)
            });
        }

        // POST: UserTasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Priority,Status,Category,DueDate")] UserTask userTask)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            ModelState.Remove("UserId");
            ModelState.Remove("ApplicationUser");

            if (ModelState.IsValid)
            {
                userTask.UserId = user.Id;
                userTask.CreatedAt = DateTime.UtcNow;
                if (userTask.Status == Models.TaskStatus.Completed)
                {
                    userTask.CompletedAt = DateTime.UtcNow;
                }

                _context.Add(userTask);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Task created successfully!";
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
            if (user == null)
            {
                return Challenge();
            }

            var userTask = await _context.UserTasks
                .Include(t => t.SubTasks.OrderBy(s => s.CreatedAt))
                .FirstOrDefaultAsync(t => t.UserTaskId == id && t.UserId == user.Id);

            if (userTask == null)
            {
                return NotFound();
            }

            return View(userTask);
        }

        // POST: UserTasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserTaskId,Title,Description,Priority,Status,Category,DueDate")] UserTask userTask)
        {
            if (id != userTask.UserTaskId)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            ModelState.Remove("UserId");
            ModelState.Remove("ApplicationUser");

            var existingTask = await _context.UserTasks
                .Include(t => t.SubTasks)
                .FirstOrDefaultAsync(t => t.UserTaskId == id && t.UserId == user.Id);

            if (existingTask == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingTask.Title = userTask.Title;
                    existingTask.Description = userTask.Description;
                    existingTask.Priority = userTask.Priority;
                    existingTask.Category = userTask.Category;
                    existingTask.DueDate = userTask.DueDate;

                    // Handle completion timestamp
                    if (existingTask.Status != Models.TaskStatus.Completed && userTask.Status == Models.TaskStatus.Completed)
                    {
                        existingTask.CompletedAt = DateTime.UtcNow;
                    }
                    else if (existingTask.Status == Models.TaskStatus.Completed && userTask.Status != Models.TaskStatus.Completed)
                    {
                        existingTask.CompletedAt = null;
                    }
                    existingTask.Status = userTask.Status;

                    _context.Update(existingTask);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Task updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserTaskExists(userTask.UserTaskId, user.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            userTask.SubTasks = existingTask.SubTasks;
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
            if (user == null)
            {
                return Challenge();
            }

            var userTask = await _context.UserTasks
                .Include(t => t.SubTasks)
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
            if (user == null)
            {
                return Challenge();
            }

            var userTask = await _context.UserTasks.FirstOrDefaultAsync(t => t.UserTaskId == id && t.UserId == user.Id);
            if (userTask != null)
            {
                _context.UserTasks.Remove(userTask);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Task deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: UserTasks/ToggleStatus/5 (AJAX 1-click & Kanban drag)
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, Models.TaskStatus? targetStatus)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var task = await _context.UserTasks.FirstOrDefaultAsync(t => t.UserTaskId == id && t.UserId == user.Id);
            if (task == null)
            {
                return NotFound();
            }

            if (targetStatus.HasValue)
            {
                task.Status = targetStatus.Value;
            }
            else
            {
                // Toggle between Completed and Todo
                task.Status = task.Status == Models.TaskStatus.Completed ? Models.TaskStatus.Todo : Models.TaskStatus.Completed;
            }

            if (task.Status == Models.TaskStatus.Completed)
            {
                task.CompletedAt = DateTime.UtcNow;
            }
            else
            {
                task.CompletedAt = null;
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                newStatus = (int)task.Status,
                statusName = task.Status.ToString(),
                isCompleted = task.Status == Models.TaskStatus.Completed
            });
        }

        // POST: UserTasks/AddSubTask (AJAX)
        [HttpPost]
        public async Task<IActionResult> AddSubTask(int userTaskId, string title)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest(new { success = false, message = "Subtask title is required." });
            }

            var task = await _context.UserTasks.FirstOrDefaultAsync(t => t.UserTaskId == userTaskId && t.UserId == user.Id);
            if (task == null)
            {
                return NotFound(new { success = false, message = "Task not found." });
            }

            var subTask = new SubTask
            {
                UserTaskId = userTaskId,
                Title = title.Trim(),
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.SubTasks.Add(subTask);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                subTaskId = subTask.SubTaskId,
                title = subTask.Title,
                isCompleted = subTask.IsCompleted
            });
        }

        // POST: UserTasks/ToggleSubTask/5 (AJAX)
        [HttpPost]
        public async Task<IActionResult> ToggleSubTask(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var subTask = await _context.SubTasks
                .Include(s => s.UserTask)
                .FirstOrDefaultAsync(s => s.SubTaskId == id && s.UserTask!.UserId == user.Id);

            if (subTask == null)
            {
                return NotFound();
            }

            subTask.IsCompleted = !subTask.IsCompleted;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                subTaskId = subTask.SubTaskId,
                isCompleted = subTask.IsCompleted
            });
        }

        // POST: UserTasks/DeleteSubTask/5 (AJAX)
        [HttpPost]
        public async Task<IActionResult> DeleteSubTask(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var subTask = await _context.SubTasks
                .Include(s => s.UserTask)
                .FirstOrDefaultAsync(s => s.SubTaskId == id && s.UserTask!.UserId == user.Id);

            if (subTask == null)
            {
                return NotFound();
            }

            _context.SubTasks.Remove(subTask);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // POST: UserTasks/SendTaskSummaryEmail (Using Gmail SMTP)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendTaskSummaryEmail()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
            {
                return Challenge();
            }

            var tasks = await _context.UserTasks
                .Include(t => t.SubTasks)
                .Where(t => t.UserId == user.Id)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            var pendingTasks = tasks.Where(t => t.Status != Models.TaskStatus.Completed).ToList();
            var overdueTasks = tasks.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date < DateTime.Today && t.Status != Models.TaskStatus.Completed).ToList();
            var todayTasks = tasks.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == DateTime.Today && t.Status != Models.TaskStatus.Completed).ToList();

            var subject = $"Your TaskManager Summary & Upcoming Deadlines ({DateTime.Today:MMM dd, yyyy})";

            var taskItemsHtml = new StringBuilder();
            if (!pendingTasks.Any())
            {
                taskItemsHtml.Append("<p style='color: #10b981; font-weight: bold;'>🎉 All caught up! You have 0 pending tasks.</p>");
            }
            else
            {
                taskItemsHtml.Append("<table width='100%' style='border-collapse: collapse; margin-top: 15px;'>");
                taskItemsHtml.Append("<tr style='background: #1e293b; color: #94a3b8; font-size: 12px; text-transform: uppercase;'>");
                taskItemsHtml.Append("<th style='padding: 10px; text-align: left;'>Task</th>");
                taskItemsHtml.Append("<th style='padding: 10px; text-align: left;'>Priority</th>");
                taskItemsHtml.Append("<th style='padding: 10px; text-align: left;'>Category</th>");
                taskItemsHtml.Append("<th style='padding: 10px; text-align: left;'>Due Date</th>");
                taskItemsHtml.Append("</tr>");

                foreach (var t in pendingTasks)
                {
                    var isOverdue = t.DueDate.HasValue && t.DueDate.Value.Date < DateTime.Today;
                    var isDueToday = t.DueDate.HasValue && t.DueDate.Value.Date == DateTime.Today;

                    string dateStyle = isOverdue ? "color: #f43f5e; font-weight: bold;" : (isDueToday ? "color: #f59e0b; font-weight: bold;" : "color: #94a3b8;");
                    string dateText = t.DueDate.HasValue ? t.DueDate.Value.ToString("MMM dd, yyyy") : "No deadline";

                    taskItemsHtml.Append("<tr style='border-bottom: 1px solid rgba(255,255,255,0.06); font-size: 14px;'>");
                    taskItemsHtml.Append($"<td style='padding: 12px 10px; color: #f8fafc; font-weight: 600;'>{t.Title}</td>");
                    taskItemsHtml.Append($"<td style='padding: 12px 10px; color: #cbd5e1;'>{t.Priority}</td>");
                    taskItemsHtml.Append($"<td style='padding: 12px 10px; color: #cbd5e1;'>{t.Category}</td>");
                    taskItemsHtml.Append($"<td style='padding: 12px 10px; {dateStyle}'>{dateText}</td>");
                    taskItemsHtml.Append("</tr>");
                }
                taskItemsHtml.Append("</table>");
            }

            var emailHtml = $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='margin: 0; padding: 0; background-color: #0f172a; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; color: #f8fafc;'>
    <table role='presentation' width='100%' cellspacing='0' cellpadding='0' style='background-color: #0f172a; padding: 30px 15px;'>
        <tr>
            <td align='center'>
                <table role='presentation' width='100%' style='max-width: 600px; background: #111827; border-radius: 16px; border: 1px solid rgba(255, 255, 255, 0.1); box-shadow: 0 20px 40px rgba(0,0,0,0.5); overflow: hidden;'>
                    <tr>
                        <td style='padding: 25px 30px; background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 50%, #d946ef 100%); color: #ffffff; text-align: center;'>
                            <h1 style='margin: 0; font-size: 22px; font-weight: 800;'>TaskManager Daily Digest</h1>
                            <p style='margin: 5px 0 0; opacity: 0.9; font-size: 14px;'>Here is your current productivity snapshot</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 25px 30px;'>
                            <div style='display: flex; gap: 10px; margin-bottom: 20px; text-align: center;'>
                                <div style='background: #1f2937; padding: 12px; border-radius: 10px; flex: 1;'>
                                    <div style='font-size: 20px; font-weight: 800; color: #6366f1;'>{pendingTasks.Count}</div>
                                    <div style='font-size: 11px; color: #94a3b8; text-transform: uppercase;'>Pending</div>
                                </div>
                                <div style='background: #1f2937; padding: 12px; border-radius: 10px; flex: 1;'>
                                    <div style='font-size: 20px; font-weight: 800; color: #f59e0b;'>{todayTasks.Count}</div>
                                    <div style='font-size: 11px; color: #94a3b8; text-transform: uppercase;'>Due Today</div>
                                </div>
                                <div style='background: #1f2937; padding: 12px; border-radius: 10px; flex: 1;'>
                                    <div style='font-size: 20px; font-weight: 800; color: #f43f5e;'>{overdueTasks.Count}</div>
                                    <div style='font-size: 11px; color: #94a3b8; text-transform: uppercase;'>Overdue</div>
                                </div>
                            </div>
                            <h3 style='margin: 20px 0 10px; font-size: 16px; color: #ffffff;'>Your Active Task List</h3>
                            {taskItemsHtml}
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 15px 30px; background: #0b0f19; text-align: center; border-top: 1px solid rgba(255,255,255,0.05); font-size: 12px; color: #64748b;'>
                            Sent with TaskManager Automation via Gmail SMTP &copy; {DateTime.UtcNow.Year}
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

            try
            {
                await _emailSender.SendEmailAsync(user.Email, subject, emailHtml);
                TempData["SuccessMessage"] = $"Task digest sent to {user.Email} via Gmail SMTP!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send task summary email to {Email}", user.Email);
                TempData["ErrorMessage"] = "Could not send email reminder. Please check your SMTP settings.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: UserTasks/ExportCsv
        public async Task<IActionResult> ExportCsv()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var tasks = await _context.UserTasks
                .Include(t => t.SubTasks)
                .Where(t => t.UserId == user.Id)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("TaskID,Title,Status,Priority,Category,DueDate,CompletedDate,CreatedAt,SubtasksTotal,SubtasksDone,Description");

            foreach (var t in tasks)
            {
                var titleClean = EscapeCsv(t.Title);
                var descClean = EscapeCsv(t.Description ?? "");
                var dueDateStr = t.DueDate?.ToString("yyyy-MM-dd") ?? "";
                var completedDateStr = t.CompletedAt?.ToString("yyyy-MM-dd HH:mm") ?? "";
                var createdDateStr = t.CreatedAt.ToString("yyyy-MM-dd HH:mm");
                var totalSub = t.SubTasks.Count;
                var doneSub = t.SubTasks.Count(s => s.IsCompleted);

                sb.AppendLine($"{t.UserTaskId},{titleClean},{t.Status},{t.Priority},{t.Category},{dueDateStr},{completedDateStr},{createdDateStr},{totalSub},{doneSub},{descClean}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"TaskManager_Export_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
        }

        // GET: UserTasks/PrintReport
        public async Task<IActionResult> PrintReport()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var tasks = await _context.UserTasks
                .Include(t => t.SubTasks)
                .Where(t => t.UserId == user.Id)
                .OrderBy(t => t.Status)
                .ThenBy(t => t.DueDate)
                .ToListAsync();

            ViewBag.UserName = user.DisplayName ?? user.UserName;
            ViewBag.UserEmail = user.Email;
            ViewBag.GeneratedAt = DateTime.Now;

            return View(tasks);
        }

        private bool UserTaskExists(int id, string userId)
        {
            return _context.UserTasks.Any(e => e.UserTaskId == id && e.UserId == userId);
        }

        private static string EscapeCsv(string field)
        {
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }
    }
}

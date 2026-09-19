// Models/UserTask.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Models
{
    public enum TaskStatus
    {
        [Display(Name = "To Do")]
        Todo = 0,

        [Display(Name = "In Progress")]
        InProgress = 1,

        [Display(Name = "Completed")]
        Completed = 2
    }

    public enum TaskPriority
    {
        [Display(Name = "Low")]
        Low = 0,

        [Display(Name = "Medium")]
        Medium = 1,

        [Display(Name = "High")]
        High = 2,

        [Display(Name = "Urgent")]
        Urgent = 3
    }

    public enum TaskCategory
    {
        [Display(Name = "General")]
        General = 0,

        [Display(Name = "Work")]
        Work = 1,

        [Display(Name = "Personal")]
        Personal = 2,

        [Display(Name = "Study")]
        Study = 3,

        [Display(Name = "Finance")]
        Finance = 4,

        [Display(Name = "Health")]
        Health = 5
    }

    public class UserTask
    {
        [Key]
        public int UserTaskId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Display(Name = "Status")]
        public TaskStatus Status { get; set; } = TaskStatus.Todo;

        [Display(Name = "Priority")]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        [Display(Name = "Category")]
        public TaskCategory Category { get; set; } = TaskCategory.General;

        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Completed Date")]
        public DateTime? CompletedAt { get; set; }

        // Foreign key to associate task with a user
        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? ApplicationUser { get; set; }

        // Subtasks navigation property
        public ICollection<SubTask> SubTasks { get; set; } = new List<SubTask>();
    }
}

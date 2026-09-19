// Models/SubTask.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Models
{
    public class SubTask
    {
        [Key]
        public int SubTaskId { get; set; }

        [Required]
        [ForeignKey("UserTask")]
        public int UserTaskId { get; set; }

        public UserTask? UserTask { get; set; }

        [Required(ErrorMessage = "Subtask title is required")]
        [StringLength(200, ErrorMessage = "Subtask cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        public bool IsCompleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

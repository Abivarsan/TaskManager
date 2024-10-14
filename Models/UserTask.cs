// Models/UserTask.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Models
{
    public class UserTask
    {
        [Key]
        public int UserTaskId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        // Foreign key to associate task with a user
        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }
}

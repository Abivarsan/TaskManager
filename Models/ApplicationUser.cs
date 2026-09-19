// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(50, ErrorMessage = "Display name cannot exceed 50 characters")]
        [Display(Name = "Display Name")]
        public string? DisplayName { get; set; }

        [StringLength(20)]
        [Display(Name = "Avatar Theme")]
        public string? AvatarColor { get; set; } = "primary"; // primary, accent, success, warning, danger, purple

        // Navigation property for the tasks associated with the user
        public ICollection<UserTask> UserTasks { get; set; } = new List<UserTask>();
    }
}

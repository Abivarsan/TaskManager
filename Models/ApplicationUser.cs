// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace TaskManager.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Navigation property for the tasks associated with the user
        public ICollection<UserTask> UserTasks { get; set; }
    }
}

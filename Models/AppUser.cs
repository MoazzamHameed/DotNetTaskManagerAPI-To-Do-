using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace TaskManagerAPI.Models
{
    public class AppUser : IdentityUser
    {
        public ICollection<TaskItem> Tasks { get; set; }    
    }
}

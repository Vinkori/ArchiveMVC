using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace ArchiveDomain.Model
{
    public class User : IdentityUser
    {
        public string? Name { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public virtual ICollection<Poetry> LikedPoems { get; set; } = new List<Poetry>();
        public virtual ICollection<Poetry> AddedPoems { get; set; } = new List<Poetry>(); // Поезії, додані користувачем
    }
}
using ArchiveDomain.Model;
using Microsoft.AspNetCore.Identity;

namespace ArchiveInfrastructure.Models
{
    public class User : IdentityUser
    {
        public string? Name { get; set; } // <-- Додаємо ім’я користувача
        public DateOnly? DateOfBirth { get; set; }
        public ICollection<Poetry> LikedPoems { get; set; } = new List<Poetry>();

    }
}
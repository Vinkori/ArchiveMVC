using ArchiveDomain.Model;

namespace ArchiveInfrastructure.Models
{
    public class UserPoetryLike
    {
        public string UserId { get; set; } = null!;
        public int PoetryId { get; set; }

        public User User { get; set; } = null!;
        public Poetry Poetry { get; set; } = null!;
    }
}

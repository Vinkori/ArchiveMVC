using System.ComponentModel.DataAnnotations;

namespace ArchiveInfrastructure.ViewModels
{
    public class EditProfileViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Email є обов’язковим")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Ім’я є обов’язковим")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Ім’я користувача є обов’язкове")]
        public string UserName { get; set; }
    }
}

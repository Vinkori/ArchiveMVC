using System.ComponentModel.DataAnnotations;

namespace ArchiveInfrastructure.ViewModels
{
    public class EditProfileViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Email є обов’язковим")]
        [EmailAddress(ErrorMessage = "Некоректний формат email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Ім’я є обов’язковим")]
        [StringLength(100, ErrorMessage = "Ім’я не може бути довшим за 100 символів")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Ім’я користувача є обов’язкове")]
        [StringLength(100, ErrorMessage = "Ім’я користувача не може бути довшим за 100 символів")]
        public string UserName { get; set; }
    }
}
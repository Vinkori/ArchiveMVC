using System.ComponentModel.DataAnnotations;

namespace ArchiveInfrastructure.ViewModels
{
    public class CreateRoleViewModel
    {
        [Required(ErrorMessage = "Поле 'Назва ролі' є обов’язковим.")]
        [StringLength(50, ErrorMessage = "Назва ролі не може перевищувати 50 символів.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Назва ролі може містити лише літери, цифри та підкреслення.")]
        [Display(Name = "Назва ролі")]
        public string RoleName { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace ArchiveInfrastructure
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }


        [Display(Name = "Номер телефону")]
        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; set; } // Optional

        [Display(Name = "Дата народження")]
        public DateOnly? DateOfBirth { get; set; } // Optional

        [Display(Name = "Ім’я")]
        [StringLength(100, ErrorMessage = "Ім’я не може бути довшим за 100 символів")]
        public string? Name { get; set; } // <-- Нова властивість


        [Display(Name = "Username")]
        [RegularExpression(@"^\S*$", ErrorMessage = "Ім'я користувача не може містити пробіли.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Ім’я має бути від 3 до 20 символів")]

        //[RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Допустимі лише латинські літери, цифри та підкреслення")]
        [Required(ErrorMessage = "Поле Username є обов’язковим")]
        public string Username { get; set; } // <-- Username залишаємо

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Підтвердження паролю")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Паролі не збігаються.")]
        public string PasswordConfirm { get; set; }

    }
}
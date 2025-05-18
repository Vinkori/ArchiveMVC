using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArchiveDomain.Model;

public partial class Author : Entity
{
    [Required(ErrorMessage = "Поле 'Ім’я' є обов’язковим.")]
    [StringLength(50, ErrorMessage = "Ім’я не може перевищувати 50 символів.")]
    [RegularExpression(@"^[a-zA-Zа-яА-Я\s-]+$", ErrorMessage = "Ім’я може містити лише літери, пробіли та дефіси.")]
    [Display(Name = "Ім’я")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Поле 'Прізвище' є обов’язковим.")]
    [StringLength(50, ErrorMessage = "Прізвище не може перевищувати 50 символів.")]
    [RegularExpression(@"^[a-zA-Zа-яА-Я\s-]+$", ErrorMessage = "Прізвище може містити лише літери, пробіли та дефіси.")]
    [Display(Name = "Прізвище")]
    public string LastName { get; set; } = null!;

    public virtual ICollection<Poetry> Poetries { get; set; } = new List<Poetry>();
}
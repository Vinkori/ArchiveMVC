using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArchiveDomain.Model;

public partial class Form : Entity
{
    [Required(ErrorMessage = "Поле 'Жанри поезії' є обов’язковим.")]
    [StringLength(50, ErrorMessage = "Назва жанру не може перевищувати 50 символів.")]
    [RegularExpression(@"^[a-zA-Zа-яА-Я\s-]+$", ErrorMessage = "Назва жанру може містити лише літери, пробіли та дефіси.")]
    [Display(Name = "Жанри поезії")]
    public string FormName { get; set; } = null!;

    public virtual ICollection<Poetry> Poetries { get; set; } = new List<Poetry>();
}
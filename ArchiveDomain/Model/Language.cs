using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArchiveDomain.Model;

public partial class Language : Entity
{
    [Required(ErrorMessage = "Поле 'Мова' є обов’язковим.")]
    [StringLength(50, ErrorMessage = "Назва мови не може перевищувати 50 символів.")]
    [RegularExpression(@"^[a-zA-Zа-яА-Я\s-]+$", ErrorMessage = "Назва мови може містити лише літери, пробіли та дефіси.")]
    [Display(Name = "Мова")]
    public string Language1 { get; set; } = null!;

    public virtual ICollection<Poetry> Poetries { get; set; } = new List<Poetry>();
}
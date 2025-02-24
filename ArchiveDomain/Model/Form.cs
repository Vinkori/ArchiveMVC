using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArchiveDomain.Model;

public partial class Form : Entity
{
    [Required(ErrorMessage = "The field should not be empty")]
    [Display(Name = "Жанри поезії")]
    public string FormName { get; set; } = null!;

    public virtual ICollection<Poetry> Poetries { get; set; } = new List<Poetry>();
}

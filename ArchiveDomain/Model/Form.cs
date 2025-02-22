using System;
using System.Collections.Generic;

namespace ArchiveDomain.Model;

public partial class Form
{
    public int Id { get; set; }

    public string FormName { get; set; } = null!;

    public virtual ICollection<Poetry> Poetries { get; set; } = new List<Poetry>();
}

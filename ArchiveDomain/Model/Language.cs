using System;
using System.Collections.Generic;

namespace ArchiveDomain.Model;

public partial class Language : Entity
{

    public string Language1 { get; set; } = null!;

    public virtual ICollection<Poetry> Poetries { get; set; } = new List<Poetry>();
}

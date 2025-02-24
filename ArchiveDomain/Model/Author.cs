using System;
using System.Collections.Generic;

namespace ArchiveDomain.Model;

public partial class Author : Entity
{

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public virtual ICollection<Poetry> Poetries { get; set; } = new List<Poetry>();
}

using System;
using System.Collections.Generic;

namespace ArchiveDomain.Model;

public partial class Reader
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public virtual ICollection<Poetry> Poetries { get; set; } = new List<Poetry>();
}

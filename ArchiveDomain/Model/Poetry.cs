using System;
using System.Collections.Generic;

namespace ArchiveDomain.Model;

public partial class Poetry : Entity
{

    public int AuthorId { get; set; }

    public string Title { get; set; } = null!;

    public string Text { get; set; } = null!;

    public DateTime PublicationDate { get; set; }

    public int LanguageId { get; set; }

    public int AdminId { get; set; }

    public virtual Admin Admin { get; set; } = null!;

    public virtual Author Author { get; set; } = null!;

    public virtual Language Language { get; set; } = null!;

    public virtual ICollection<Form> Forms { get; set; } = new List<Form>();

    public virtual ICollection<Reader> Readers { get; set; } = new List<Reader>();
}

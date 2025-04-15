using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ArchiveDomain.Model
{
    public partial class Poetry : Entity
    {
        [Display(Name = "Автор")]
        public int AuthorId { get; set; }

        [Display(Name = "Назва")]
        public string Title { get; set; } = null!;

        [Display(Name = "Текст")]
        public string Text { get; set; } = null!;

        [Display(Name = "Дата публікації")]
        public DateTime PublicationDate { get; set; }

        [Display(Name = "Мова")]
        public int LanguageId { get; set; }

        [Display(Name = "Додано користувачем")]
        public string AddedByUserId { get; set; } = null!;

        public virtual User AddedByUser { get; set; } = null!;
        public virtual Author Author { get; set; } = null!;
        public virtual Language Language { get; set; } = null!;
        public virtual ICollection<Form> Forms { get; set; } = new List<Form>();
        public virtual ICollection<User> LikedByUsers { get; set; } = new List<User>();
    }
}
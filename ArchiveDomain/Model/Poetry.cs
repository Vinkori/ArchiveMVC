using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ArchiveDomain.Model
{
    public partial class Poetry : Entity
    {
        [Required(ErrorMessage = "Поле 'Автор' є обов’язковим.")]
        [Display(Name = "Автор")]
        public int AuthorId { get; set; }

        [Required(ErrorMessage = "Поле 'Назва' є обов’язковим.")]
        [StringLength(255, ErrorMessage = "Назва не може перевищувати 255 символів.")]
        [Display(Name = "Назва")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Поле 'Текст' є обов’язковим.")]
        [StringLength(10000, ErrorMessage = "Текст не може перевищувати 10000 символів.")]
        [Display(Name = "Текст")]
        public string Text { get; set; } = null!;

        [Required(ErrorMessage = "Поле 'Дата публікації' є обов’язковим.")]
        [Display(Name = "Дата публікації")]
        public DateTime PublicationDate { get; set; }

        [Required(ErrorMessage = "Поле 'Мова' є обов’язковим.")]
        [Display(Name = "Мова")]
        public int LanguageId { get; set; }

        [Required(ErrorMessage = "Поле 'Додано користувачем' є обов’язковим.")]
        [Display(Name = "Додано користувачем")]
        public string AddedByUserId { get; set; } = null!;
        [Display(Name = "Додано користувачем")]
        public virtual User AddedByUser { get; set; } = null!;
        [Display(Name = "Автор")]
        public virtual Author Author { get; set; } = null!;
        [Display(Name = "Мова")]
        public virtual Language Language { get; set; } = null!;
        [Display(Name = "Жанри")]
        public virtual ICollection<Form> Forms { get; set; } = new List<Form>();
        [Display(Name = "Вподобано користувачами")]
        public virtual ICollection<User> LikedByUsers { get; set; } = new List<User>();
    }
}
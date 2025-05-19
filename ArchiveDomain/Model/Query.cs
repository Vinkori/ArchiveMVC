using System;
using System.Collections.Generic;

namespace ArchiveDomain.Model
{
    public class Query
    {
        // Поля для введення параметрів запитів
        public string LastName { get; set; } // Query1: Прізвище автора
        public string Keyword { get; set; } // Query1: Ключове слово в назві
        public int? MinLikes { get; set; } // Query1: Мінімальна кількість лайків
        public string LanguageName { get; set; } // Query2, Query5: Назва мови
        public DateTime? AfterDate { get; set; } // Query3: Дата після якої
        public string FormName { get; set; } // Query4: Назва жанру
        public string AdminName { get; set; } // Query5: Логін адміністратора
        public string UserName { get; set; } // Query6:  користувача
        public int? AuthorId { get; set; } // Query8: ID автора


        // Поля для результатів і помилок
        public string QueryName { get; set; } // Назва запиту (Q1, Q2, ..., Q8)
        public int ErrorFlag { get; set; } // Прапорець помилки
        public string ErrorName { get; set; } // Повідомлення про помилку

        // Результати для різних запитів
        public List<PoetryInfo> Poems { get; set; } // Query1, Query5: Список віршів
        public List<AuthorInfo> Authors { get; set; } // Query2: Список авторів
        public List<string> Genres { get; set; } // Query3: Список жанрів
        public List<UserInfo> Users { get; set; } // Query4, Query6, Query8: Список користувачів
        public List<UserPair> UserPairs { get; set; } // Query7: Пари користувачів

        // Вкладені класи для структуризації даних
        public class PoetryInfo
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Text { get; set; }
            public DateTime PublicationDate { get; set; }
            public string AuthorName { get; set; }
            public string LanguageName { get; set; }
            public string AdminName { get; set; }
        }

        public class AuthorInfo
        {
            public int Id { get; set; }
            public string FullName { get; set; }
        }

        public class UserInfo
        {
            public string Id { get; set; }
            public string UserName { get; set; }
        }

        public class UserPair
        {
            public string UserA { get; set; }
            public string UserB { get; set; }
        }
    }
}
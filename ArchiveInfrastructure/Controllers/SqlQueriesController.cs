using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ArchiveDomain.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace ArchiveInfrastructure.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SqlQueriesController : Controller
    {
        private readonly DbarchiveContext _context;
        private readonly string _connectionString;
        private readonly string _queriesPath;

        private const string Q1_PATH = @"D:\Projects\BdLab02\ArchiveInfrastructure\Queries\Query1.sql";
        private const string Q2_PATH = @"D:\Projects\BdLab02\ArchiveInfrastructure\Queries\Query2.sql";
        private const string Q3_PATH = @"D:\Projects\BdLab02\ArchiveInfrastructure\Queries\Query3.sql";
        private const string Q4_PATH = @"D:\Projects\BdLab02\ArchiveInfrastructure\Queries\Query4.sql";
        private const string Q5_PATH = @"D:\Projects\BdLab02\ArchiveInfrastructure\Queries\Query5.sql";
        private const string Q6_PATH = @"D:\Projects\BdLab02\ArchiveInfrastructure\Queries\Query6.sql";
        private const string Q7_PATH = @"D:\Projects\BdLab02\ArchiveInfrastructure\Queries\Query7.sql";
        private const string Q8_PATH = @"D:\Projects\BdLab02\ArchiveInfrastructure\Queries\Query8.sql";

        private const string ERR_POEMS = "Вірші, що задовольняють умову, відсутні.";
        private const string ERR_AUTHORS = "Автори, що задовольняють умову, відсутні.";
        private const string ERR_GENRES = "Жанри, що задовольняють умову, відсутні.";
        private const string ERR_USERS = "Користувачі, що задовольняють умову, відсутні.";
        private const string ERR_PAIRS = "Пари користувачів, що задовольняють умову, відсутні.";

        public SqlQueriesController(DbarchiveContext context, IWebHostEnvironment env)
        {
            _context = context;
            _connectionString = context.Database.GetDbConnection().ConnectionString;
            _queriesPath = Path.Combine(env.ContentRootPath, "Queries");
        }

        public IActionResult Index()
        {
            return View(new Query());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Query1(Query queryModel)
        {
            if (string.IsNullOrEmpty(queryModel.LastName) || string.IsNullOrEmpty(queryModel.Keyword) || !queryModel.MinLikes.HasValue)
            {
                ViewBag.ErrorFlag = 1;
                ViewBag.QuantityError = "Усі поля для запиту 1 є обов'язковими.";
                return View("Index", queryModel);
            }

            string query = System.IO.File.ReadAllText(Q1_PATH);
            queryModel.Poems = new List<Query.PoetryInfo>();
            queryModel.QueryName = "Q1";
            queryModel.ErrorFlag = 0;
            queryModel.ErrorName = null;

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LastName", queryModel.LastName);
                        command.Parameters.AddWithValue("@Keyword", queryModel.Keyword);
                        command.Parameters.AddWithValue("@MinLikes", queryModel.MinLikes.Value);

                        using (var reader = command.ExecuteReader())
                        {
                            int count = 0;
                            while (reader.Read())
                            {
                                queryModel.Poems.Add(new Query.PoetryInfo
                                {
                                    Id = reader.GetInt32(0),
                                    Title = reader.GetString(1),
                                    Text = reader.GetString(2),
                                    PublicationDate = reader.GetDateTime(3),
                                    AuthorName = reader.GetString(4),
                                    LanguageName = reader.GetString(5),
                                    AdminName = reader.GetString(6)
                                });
                                count++;
                            }
                            if (count == 0)
                            {
                                queryModel.ErrorFlag = 1;
                                queryModel.ErrorName = ERR_POEMS;
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                queryModel.ErrorFlag = 1;
                queryModel.ErrorName = "Помилка: " + ex.Message;
            }

            return View("Results", queryModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Query2(Query queryModel)
        {
            if (string.IsNullOrEmpty(queryModel.LanguageName))
            {
                ViewBag.ErrorFlag = 1;
                ViewBag.QuantityError = "Назва мови для запиту 2 є обов'язковою.";
                return View("Index", queryModel);
            }

            string query = System.IO.File.ReadAllText(Q2_PATH);
            queryModel.Authors = new List<Query.AuthorInfo>();
            queryModel.QueryName = "Q2";
            queryModel.ErrorFlag = 0;
            queryModel.ErrorName = null;

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LanguageName", queryModel.LanguageName);

                        using (var reader = command.ExecuteReader())
                        {
                            int count = 0;
                            while (reader.Read())
                            {
                                queryModel.Authors.Add(new Query.AuthorInfo
                                {
                                    Id = reader.GetInt32(0),
                                    FullName = $"{reader.GetString(1)} {reader.GetString(2)}"
                                });
                                count++;
                            }
                            if (count == 0)
                            {
                                queryModel.ErrorFlag = 1;
                                queryModel.ErrorName = ERR_AUTHORS;
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                queryModel.ErrorFlag = 1;
                queryModel.ErrorName = "Помилка: " + ex.Message;
            }

            return View("Results", queryModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Query3(Query queryModel)
        {
            if (!queryModel.AfterDate.HasValue)
            {
                ViewBag.ErrorFlag = 1;
                ViewBag.QuantityError = "Дата для запиту 3 є обов'язковою.";
                return View("Index", queryModel);
            }

            string query = System.IO.File.ReadAllText(Q3_PATH);
            queryModel.Genres = new List<string>();
            queryModel.QueryName = "Q3";
            queryModel.ErrorFlag = 0;
            queryModel.ErrorName = null;

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AfterDate", queryModel.AfterDate.Value);

                        using (var reader = command.ExecuteReader())
                        {
                            int count = 0;
                            while (reader.Read())
                            {
                                queryModel.Genres.Add(reader.GetString(0)); // Зчитуємо лише FormName як string
                                count++;
                            }
                            if (count == 0)
                            {
                                queryModel.ErrorFlag = 1;
                                queryModel.ErrorName = ERR_GENRES;
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                queryModel.ErrorFlag = 1;
                queryModel.ErrorName = "Помилка: " + ex.Message;
            }

            return View("Results", queryModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Query4(Query queryModel)
        {
            if (string.IsNullOrEmpty(queryModel.FormName))
            {
                ViewBag.ErrorFlag = 1;
                ViewBag.QuantityError = "Назва жанру для запиту 4 є обов'язковою.";
                return View("Index", queryModel);
            }

            string query = System.IO.File.ReadAllText(Q4_PATH);
            queryModel.Users = new List<Query.UserInfo>();
            queryModel.QueryName = "Q4";
            queryModel.ErrorFlag = 0;
            queryModel.ErrorName = null;

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FormName", queryModel.FormName);

                        using (var reader = command.ExecuteReader())
                        {
                            int count = 0;
                            while (reader.Read())
                            {
                                queryModel.Users.Add(new Query.UserInfo
                                {
                                    Id = reader.GetString(0),
                                    UserName = reader.GetString(1)
                                });
                                count++;
                            }
                            if (count == 0)
                            {
                                queryModel.ErrorFlag = 1;
                                queryModel.ErrorName = ERR_USERS;
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                queryModel.ErrorFlag = 1;
                queryModel.ErrorName = "Помилка: " + ex.Message;
            }

            return View("Results", queryModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Query5(Query queryModel)
        {
            if (string.IsNullOrEmpty(queryModel.AdminName) || string.IsNullOrEmpty(queryModel.LanguageName))
            {
                ViewBag.ErrorFlag = 1;
                ViewBag.QuantityError = "Усі поля для запиту 5 є обов'язковими.";
                return View("Index", queryModel);
            }

            // Validate admin exists
            if (!_context.Users.Any(u => u.UserName == queryModel.AdminName))
            {
                ViewBag.ErrorFlag = 1;
                ViewBag.QuantityError = "Адміністратор із вказаним логіном не існує.";
                return View("Index", queryModel);
            }

            string query = System.IO.File.ReadAllText(Q5_PATH);
            queryModel.Poems = new List<Query.PoetryInfo>();
            queryModel.QueryName = "Q5";
            queryModel.ErrorFlag = 0;
            queryModel.ErrorName = null;

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AdminName", queryModel.AdminName);
                        command.Parameters.AddWithValue("@LanguageName", queryModel.LanguageName);

                        using (var reader = command.ExecuteReader())
                        {
                            int count = 0;
                            while (reader.Read())
                            {
                                queryModel.Poems.Add(new Query.PoetryInfo
                                {
                                    Id = reader.GetInt32(0),
                                    Title = reader.GetString(1),
                                    Text = reader.GetString(2),
                                    PublicationDate = reader.GetDateTime(3),
                                    AuthorName = reader.GetString(4),
                                    LanguageName = reader.GetString(5),
                                    AdminName = reader.GetString(6)
                                });
                                count++;
                            }
                            if (count == 0)
                            {
                                queryModel.ErrorFlag = 1;
                                queryModel.ErrorName = ERR_POEMS;
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                queryModel.ErrorFlag = 1;
                queryModel.ErrorName = "Помилка: " + ex.Message;
            }

            return View("Results", queryModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Query6(Query queryModel)
        {
            if (string.IsNullOrEmpty(queryModel.UserName))
            {
                ViewBag.ErrorFlag = 1;
                ViewBag.QuantityError = "Логін користувача для запиту 6 є обов'язковим.";
                return View("Index", queryModel);
            }

            // Validate user exists
            if (!_context.Users.Any(u => u.UserName == queryModel.UserName))
            {
                ViewBag.ErrorFlag = 1;
                ViewBag.QuantityError = "Користувач із вказаним логіном не існує.";
                return View("Index", queryModel);
            }

            string query = System.IO.File.ReadAllText(Q6_PATH);
            queryModel.Users = new List<Query.UserInfo>();
            queryModel.QueryName = "Q6";
            queryModel.ErrorFlag = 0;
            queryModel.ErrorName = null;

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserName", queryModel.UserName);

                        using (var reader = command.ExecuteReader())
                        {
                            int count = 0;
                            while (reader.Read())
                            {
                                queryModel.Users.Add(new Query.UserInfo
                                {
                                    Id = reader.GetString(0),
                                    UserName = reader.GetString(1)
                                });
                                count++;
                            }
                            if (count == 0)
                            {
                                queryModel.ErrorFlag = 1;
                                queryModel.ErrorName = ERR_USERS;
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                queryModel.ErrorFlag = 1;
                queryModel.ErrorName = "Помилка: " + ex.Message;
            }

            return View("Results", queryModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Query7(Query queryModel)
        {
            if (string.IsNullOrEmpty(queryModel.LastName))
            {
                ViewBag.ErrorFlag = 1;
                ViewBag.QuantityError = "Прізвище автора для запиту 7 є обов'язковим.";
                return View("Index", queryModel);
            }

            string query = System.IO.File.ReadAllText(Q7_PATH);
            queryModel.Authors = new List<Query.AuthorInfo>(); // Використовуємо список авторів
            queryModel.QueryName = "Q7";
            queryModel.ErrorFlag = 0;
            queryModel.ErrorName = null;

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LastName", queryModel.LastName);

                        using (var reader = command.ExecuteReader())
                        {
                            int count = 0;
                            while (reader.Read())
                            {
                                queryModel.Authors.Add(new Query.AuthorInfo
                                {
                                    Id = reader.GetInt32(0),
                                    FullName = $"{reader.GetString(1)} {reader.GetString(2)}"
                                });
                                count++;
                            }
                            if (count == 0)
                            {
                                queryModel.ErrorFlag = 1;
                                queryModel.ErrorName = ERR_AUTHORS;
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                queryModel.ErrorFlag = 1;
                queryModel.ErrorName = "Помилка: " + ex.Message;
            }

            return View("Results", queryModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Query8(Query queryModel)
        {
            if (string.IsNullOrEmpty(queryModel.LastName))
            {
                ViewBag.ErrorFlag = 1;
                ViewBag.QuantityError = "Прізвище автора для запиту 8 є обов'язковим.";
                return View("Index", queryModel);
            }

            // Validate author exists
            if (!_context.Authors.Any(a => a.LastName == queryModel.LastName))
            {
                ViewBag.ErrorFlag = 1;
                ViewBag.QuantityError = "Автор із вказаним прізвищем не існує.";
                return View("Index", queryModel);
            }

            string query = System.IO.File.ReadAllText(Q8_PATH);
            queryModel.Users = new List<Query.UserInfo>();
            queryModel.QueryName = "Q8";
            queryModel.ErrorFlag = 0;
            queryModel.ErrorName = null;

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LastName", queryModel.LastName);

                        using (var reader = command.ExecuteReader())
                        {
                            int count = 0;
                            while (reader.Read())
                            {
                                queryModel.Users.Add(new Query.UserInfo
                                {
                                    Id = reader.GetString(0),
                                    UserName = reader.GetString(1)
                                });
                                count++;
                            }
                            if (count == 0)
                            {
                                queryModel.ErrorFlag = 1;
                                queryModel.ErrorName = ERR_USERS;
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                queryModel.ErrorFlag = 1;
                queryModel.ErrorName = "Помилка: " + ex.Message;
            }

            return View("Results", queryModel);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ArchiveDomain.Model;
using ArchiveInfrastructure.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace ArchiveInfrastructure.Controllers
{
    public class SqlQueriesController : Controller
    {
        private readonly DbarchiveContext _context;
        private readonly string _connectionString;
        private readonly string _queriesPath;

        public SqlQueriesController(DbarchiveContext context, IWebHostEnvironment env)
        {
            _context = context;
            _connectionString = context.Database.GetDbConnection().ConnectionString;
            _queriesPath = Path.Combine(env.ContentRootPath, "Queries");
        }

        public IActionResult Index()
        {
            return View(new SqlQueriesViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(SqlQueriesViewModel model, string queryType)
        {
            try
            {
                switch (queryType)
                {
                    case "Query1":
                        if (string.IsNullOrEmpty(model.Query1_LastName) || string.IsNullOrEmpty(model.Query1_Keyword) || !model.Query1_MinLikes.HasValue)
                        {
                            model.ErrorMessage = "Усі поля для запиту 1 є обов'язковими.";
                            break;
                        }
                        string sql1 = File.ReadAllText(Path.Combine(_queriesPath, "Query1.sql"));
                        var params1 = new[]
                        {
                            new SqlParameter("@LastName", model.Query1_LastName),
                            new SqlParameter("@Keyword", model.Query1_Keyword),
                            new SqlParameter("@MinLikes", model.Query1_MinLikes.Value)
                        };
                        (model.QueryResults, model.ColumnNames) = ExecuteQuery(sql1, params1);
                        model.ExecutedQuery = "Query1";
                        if (model.QueryResults.Count == 0) model.ErrorMessage = "Немає результатів.";
                        break;

                    case "Query2":
                        if (string.IsNullOrEmpty(model.Query2_LanguageName))
                        {
                            model.ErrorMessage = "Назва мови для запиту 2 є обов'язковою.";
                            break;
                        }
                        string sql2 = File.ReadAllText(Path.Combine(_queriesPath, "Query2.sql"));
                        var params2 = new[] { new SqlParameter("@LanguageName", model.Query2_LanguageName) };
                        (model.QueryResults, model.ColumnNames) = ExecuteQuery(sql2, params2);
                        model.ExecutedQuery = "Query2";
                        if (model.QueryResults.Count == 0) model.ErrorMessage = "Немає результатів.";
                        break;

                    case "Query3":
                        if (!model.Query3_AfterDate.HasValue)
                        {
                            model.ErrorMessage = "Дата для запиту 3 є обов'язковою.";
                            break;
                        }
                        string sql3 = File.ReadAllText(Path.Combine(_queriesPath, "Query3.sql"));
                        var params3 = new[] { new SqlParameter("@AfterDate", model.Query3_AfterDate.Value) };
                        (model.QueryResults, model.ColumnNames) = ExecuteQuery(sql3, params3);
                        model.ExecutedQuery = "Query3";
                        if (model.QueryResults.Count == 0) model.ErrorMessage = "Немає результатів.";
                        break;

                    case "Query4":
                        if (string.IsNullOrEmpty(model.Query4_FormName))
                        {
                            model.ErrorMessage = "Назва жанру для запиту 4 є обов'язковою.";
                            break;
                        }
                        string sql4 = File.ReadAllText(Path.Combine(_queriesPath, "Query4.sql"));
                        var params4 = new[] { new SqlParameter("@FormName", model.Query4_FormName) };
                        (model.QueryResults, model.ColumnNames) = ExecuteQuery(sql4, params4);
                        model.ExecutedQuery = "Query4";
                        if (model.QueryResults.Count == 0) model.ErrorMessage = "Немає результатів.";
                        break;

                    case "Query5":
                        if (string.IsNullOrEmpty(model.Query5_AdminName) || string.IsNullOrEmpty(model.Query5_LanguageName))
                        {
                            model.ErrorMessage = "Усі поля для запиту 5 є обов'язковими.";
                            break;
                        }
                        string sql5 = File.ReadAllText(Path.Combine(_queriesPath, "Query5.sql"));
                        var params5 = new[]
                        {
                            new SqlParameter("@AdminName", model.Query5_AdminName),
                            new SqlParameter("@LanguageName", model.Query5_LanguageName)
                        };
                        (model.QueryResults, model.ColumnNames) = ExecuteQuery(sql5, params5);
                        model.ExecutedQuery = "Query5";
                        if (model.QueryResults.Count == 0) model.ErrorMessage = "Немає результатів.";
                        break;

                    case "Query6":
                        if (string.IsNullOrEmpty(model.Query6_UserId))
                        {
                            model.ErrorMessage = "ID користувача для запиту 6 є обов'язковим.";
                            break;
                        }
                        string sql6 = File.ReadAllText(Path.Combine(_queriesPath, "Query6.sql"));
                        var params6 = new[] { new SqlParameter("@UserId", model.Query6_UserId) };
                        (model.QueryResults, model.ColumnNames) = ExecuteQuery(sql6, params6);
                        model.ExecutedQuery = "Query6";
                        if (model.QueryResults.Count == 0) model.ErrorMessage = "Немає результатів.";
                        break;

                    case "Query7":
                        string sql7 = File.ReadAllText(Path.Combine(_queriesPath, "Query7.sql"));
                        (model.QueryResults, model.ColumnNames) = ExecuteQuery(sql7, null);
                        model.ExecutedQuery = "Query7";
                        if (model.QueryResults.Count == 0) model.ErrorMessage = "Немає результатів.";
                        break;

                    case "Query8":
                        if (!model.Query8_AuthorId.HasValue)
                        {
                            model.ErrorMessage = "ID автора для запиту 8 є обов'язковим.";
                            break;
                        }
                        string sql8 = File.ReadAllText(Path.Combine(_queriesPath, "Query8.sql"));
                        var params8 = new[] { new SqlParameter("@AuthorId", model.Query8_AuthorId.Value) };
                        (model.QueryResults, model.ColumnNames) = ExecuteQuery(sql8, params8);
                        model.ExecutedQuery = "Query8";
                        if (model.QueryResults.Count == 0) model.ErrorMessage = "Немає результатів.";
                        break;

                    default:
                        model.ErrorMessage = "Невідомий тип запиту.";
                        break;
                }
            }
            catch (Exception ex)
            {
                model.ErrorMessage = $"Помилка: {ex.Message}";
            }
            return View(model);
        }

        private (List<Dictionary<string, object>>, List<string>) ExecuteQuery(string sql, SqlParameter[] parameters)
        {
            var results = new List<Dictionary<string, object>>();
            var columnNames = new List<string>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(sql, connection))
                {
                    if (parameters != null) command.Parameters.AddRange(parameters);
                    using (var reader = command.ExecuteReader())
                    {
                        for (int i = 0; i < reader.FieldCount; i++) columnNames.Add(reader.GetName(i));
                        while (reader.Read())
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++) row[reader.GetName(i)] = reader.GetValue(i);
                            results.Add(row);
                        }
                    }
                }
            }
            return (results, columnNames);
        }
    }
}
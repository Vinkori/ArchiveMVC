namespace ArchiveInfrastructure.ViewModels
{
    public class SqlQueriesViewModel
    {
        // Query1
        public string Query1_LastName { get; set; }
        public string Query1_Keyword { get; set; }
        public int? Query1_MinLikes { get; set; }

        // Query2
        public string Query2_LanguageName { get; set; }

        // Query3
        public DateTime? Query3_AfterDate { get; set; }

        // Query4
        public string Query4_FormName { get; set; }

        // Query5
        public string Query5_AdminName { get; set; }
        public string Query5_LanguageName { get; set; }

        // Query6
        public string Query6_UserId { get; set; }

        // Query8
        public int? Query8_AuthorId { get; set; }

        // Результати
        public List<Dictionary<string, object>> QueryResults { get; set; }
        public List<string> ColumnNames { get; set; }
        public string ExecutedQuery { get; set; }
        public string ErrorMessage { get; set; }
    }
}
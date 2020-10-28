using System.Collections.Generic;
using System.IO;
using static System.IO.Path;


namespace SqlInsertStatementParser
{
    class Program
    {
        private const int NumberOfRowsToInsert = 100;

        private const string Schema = "SCHEMANAME";
        private const string DbOwner = "dbo";
        private const string TableName = "TABLENAME";

        private const string ColumnNames = "[Column1], [Column2], [Column3]";
        private const char ColumnNamesSeparator = ',';

        private const string Path = @"C:\MyLocation\";
        private const string FileName = "myfile.csv";

        private const bool ShouldTruncateTable = false; // be careful!

        //NOTE: lines of the csv should have been prepared in csv by replacing ' with '' then wrapping varchars (etc) with '
        //This application can't figure out what is intended to be a string or not from a raw csv.
        static void Main(string[] args)
        {
            var columnNamesArray = ColumnNames.Split(ColumnNamesSeparator);

            var oldFile = $"{Path}{FileName}";
            var newFile = $"{Path}New{FileName}";
            newFile = ChangeExtension(newFile, ".sql"); //can use csv from .txt or .csv
            
            int i = 0;
            using (StreamWriter newFileSw = new StreamWriter(newFile))
            {
                UsingStatement(newFileSw);

                TruncateTableIfYouAreSure(newFileSw, false);

                var content = File.ReadAllLines(oldFile);
                foreach (var line in content)
                {
                    var first = InsertIntoValues(i, newFileSw, Schema, TableName, columnNamesArray, DbOwner);

                    

                    newFileSw.WriteLine($"{first}{line})");

                    i++;

                    AppendVoidIfApplicable(i, newFileSw);
                }
            }
        }

        public static void UsingStatement(StreamWriter newFileSw)
        {
            newFileSw.WriteLine($"USE {Schema}");
            newFileSw.WriteLine("GO");
        }

        public static void TruncateTableIfYouAreSure(StreamWriter newFileSw, bool areYouSure)
        {
            //again, be careful!
            if (areYouSure == true && ShouldTruncateTable == true)
            {
                newFileSw.WriteLine();
                newFileSw.WriteLine($"TRUNCATE TABLE {Schema}.{DbOwner}.{TableName}");
                newFileSw.WriteLine("GO");
            }
        }

        public static string InsertIntoValues(int i, StreamWriter newFileSw, string schema, string tableName, IEnumerable<string> columnNames, string dbOwner)
        {
            if (i == 0 || i % NumberOfRowsToInsert == 0)
            {
                newFileSw.WriteLine();
                newFileSw.WriteLine($"INSERT INTO {schema}.{dbOwner}.{tableName}");
                newFileSw.WriteLine($"\t({string.Join(",", columnNames)})");
                newFileSw.WriteLine("VALUES");
                return "\t(";

            }
            else
                return "\t, (";
        }

        public static void AppendVoidIfApplicable(int i, StreamWriter newFileSw)
        {
            if (i % NumberOfRowsToInsert == 0)
            {
                newFileSw.WriteLine("GO");
            }
        }
    }
}

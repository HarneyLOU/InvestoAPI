using CsvHelper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HistoricDataImporter
{
    public class StockDataImporter
    {
        private readonly HttpClient _client;
        private readonly string _url;
        public StockDataImporter(HttpClient client)
        {
            _client = client;
            _url = "https://stooq.com/q/d/l/?s={0}&i=d";
        }

        async public Task UpdateDailyStocks()
        {
            string queryString =
                    "SELECT CompanyId, Symbol FROM dbo.Companies;";
            using (SqlConnection connection = new SqlConnection(
                       "Server=tcp:ybbe46vwb7n4.database.windows.net,1433;Initial Catalog=testest;Persist Security Info=False;User ID=Admin123;Password=adminek123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
            {
                SqlCommand command = new SqlCommand(
                    queryString, connection);
                connection.Open();
                Dictionary<string, string> symbols = new Dictionary<string, string>();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        symbols.Add(reader[0].ToString(), reader[1].ToString());
                    }
                }
                foreach(var symbol in symbols) {
                    string insertQuery = "";
                    string today = DateTime.Now.ToString("yyyy MM dd").Replace(" ", "");
                    var dt = await GetDatatable(symbol.Value, "20201030", today);
                    foreach(DataRow row in dt.Rows)
                    {
                        insertQuery += String.Format("INSERT INTO [dbo].[Stocks] VALUES (" +
                            "{0},'{1}',{2},{3},{4},{5},{6})\n", 
                            symbol.Key, 
                            row.Field<string>("Date"),
                            row.Field<string>("Open"),
                            row.Field<string>("High"),
                            row.Field<string>("Low"),
                            row.Field<string>("Close"),
                            row.Field<string>("Volume"));
                    }
                    SqlCommand insertCommnad = new SqlCommand(insertQuery, connection);
                    try
                    {
                        insertCommnad.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    
                }

            }
        }

        async public Task<DataTable> GetDatatable(string symbol, string from = null, string to = null)
        {
            string url = String.Format(_url, $"{symbol}.US");
            if(!String.IsNullOrEmpty(from) && !String.IsNullOrEmpty(to))
            {
                url += String.Format("&d1={0}&d2={1}", from, to);
            }
            HttpResponseMessage response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(responseBody, true);
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.MissingFieldFound = null;
                using (var dr = new CsvDataReader(csv))
                {
                    var dt = new DataTable();
                    dt.Load(dr);
                    DataColumn newColumn = new DataColumn("Symbol", typeof(string));
                    newColumn.DefaultValue = symbol;
                    dt.Columns.Add(newColumn);
                    return dt;
                }
            }
        }
        static void InsertBulk(DataTable csvFileData)
        {
            using (SqlConnection dbConnection = new SqlConnection("Server=tcp:ybbe46vwb7n4.database.windows.net,1433;Initial Catalog=testest;Persist Security Info=False;User ID=Admin123;Password=adminek123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
            {
                dbConnection.Open();
                using (SqlBulkCopy s = new SqlBulkCopy(dbConnection))
                {
                    s.DestinationTableName = "StockTemp";
                    foreach (var column in csvFileData.Columns)
                        s.ColumnMappings.Add(column.ToString(), column.ToString());
                    s.WriteToServer(csvFileData);
                }
            }
        }

        static public void Transform()
        {
            using (SqlConnection dbConnection = new SqlConnection("Server=tcp:ybbe46vwb7n4.database.windows.net,1433;Initial Catalog=testest;Persist Security Info=False;User ID=Admin123;Password=adminek123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
            {
                dbConnection.Open();
                string transformQuery = @"INSERT INTO Stocks SELECT CompanyId
                                                  ,[Date]
                                                  ,[Open]
                                                  ,[High]
                                                  ,[Low]
                                                  ,[Close]
                                                  ,ISNULL([Volume], 0)
                FROM[dbo].[StockTemp] sh JOIN[dbo].[Companies] c ON sh.Symbol = c.Symbol";
                SqlCommand command = new SqlCommand(transformQuery, dbConnection);
                command.CommandTimeout = 200;
                command.ExecuteNonQuery();
            }
        }



        public class Stock
        {
            public string Date { get; set; }
            public string Open { get; set; }
            public string High { get; set; }
            public string Low { get; set; }
            public string Close { get; set; }
            public string Volume { get; set; }
        }
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using Microsoft.Data.SqlClient;

namespace FunctionApp_Managed_Instance_Connector
{
    public static class GetDataFromSQLDB
    {
        [FunctionName("GetData")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Function: GetData HTTP trigger function received a request.");

            string sql = req.Query["sql"];
            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            
            if (sql == null)
                return new BadRequestObjectResult("pass a query to execute");
            //ExecuteQuery(sql, GetConnectionString());
            var result = ExecuteQuery(GetConnectionString(), sql);
            //return (ActionResult)new OkObjectResult(result);
            return result != null
                ? (ActionResult)new OkObjectResult(result)
                : new BadRequestObjectResult("Please pass a sql query");
        }

        private static string GetConnectionString()
        {
            var sqluserid = System.Environment.GetEnvironmentVariable("sqluserid");
            var sqlpassword = System.Environment.GetEnvironmentVariable("sqlpassword");
            var server = System.Environment.GetEnvironmentVariable("server");
            var port = System.Environment.GetEnvironmentVariable("port");
            var authentication = System.Environment.GetEnvironmentVariable("authentication");
            var database = System.Environment.GetEnvironmentVariable("database");
            var trustServerCertificate = System.Environment.GetEnvironmentVariable("trustServerCertificate");
            //var connectionString = $"Data Source={server}; Authentication=Active Directory Password; UID={sqluserid}; PWD={sqlpassword}";
            var connectionString = $"Server={server}, {port};Persist Security Info=False;User ID={sqluserid};Password={sqlpassword};Database={database};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate={trustServerCertificate};Authentication={authentication};";
            return connectionString;
        }

        private static string RunQuery(string connectionString, string query)
        {
            SqlConnection sqlConnection = new SqlConnection();
            sqlConnection.ConnectionString = connectionString;
            using (SqlConnection conn = sqlConnection)
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    SqlDataReader dataReader;
                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;
                    conn.Open();
                    dataReader = cmd.ExecuteReader();
                    //var r = Serialize(dataReader);

                    var json = JsonConvert.SerializeObject(dataReader, Formatting.Indented);
                    return json;
                }
            }
        }

        private static string ExecuteQuery(string connectionString, string queryString)
        {
            queryString += " For JSON AUTO";
            using (SqlConnection connection = new SqlConnection(
                       connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(queryString, connection);

                var dataReader = command.ExecuteReader();
                var data = "";
                while (dataReader.Read())
                {
                    data += dataReader[0];
                }
                dataReader.Close();

                //var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                //return json;
                return data;
            }
        }
    }
}

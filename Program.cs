using System;
using System.Net;
using System.Net.Mail;
using System.Timers;
using System.Net.Http;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Newtonsoft.Json;



namespace APIservice
{
    // ModelJSON Class definition and parameters
    public class ModelJSON
    {
        public exchangeRates[] exchangeRates { get; set; }

        //constructors
        public ModelJSON(exchangeRates[] ExchangeRates)
        {
            this.exchangeRates = ExchangeRates;
        }
        public ModelJSON()
        {
            this.exchangeRates = null;
        }

        // Save  All CurrencyExchanges in sql database.
        public int InsertCE()
        {
            try
            {
                using (var connection = Connect())
                {
                    int conunt = 0;
                    //connection.Open();
                    foreach (exchangeRates ce in this.exchangeRates)
                    {
                        // create new insert command   
                        var command = CreateInsertCommand(connection, ce);
                        //command.ExecuteNonQuery();
                        conunt+=1;
                        Console.WriteLine(conunt.ToString()+" "+ce.key.ToString()+"  Done....");
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                // handle the exception
                Console.WriteLine("An error occurred: " + ex.Message);
                return 0;
            }
            return 1;
        }
        // UPDATE  All CurrencyExchanges in sql database.
        public int UpdateCE()
        {
            try
            {
                using (var connection = Connect())
                {
                    int conunt = 0;
                    //connection.Open();
                    foreach (exchangeRates ce in this.exchangeRates)
                    {
                        // create new insert command   
                        CreateUpdateCommand(connection, ce);
                        //command.ExecuteNonQuery();
                        conunt += 1;
                        Console.WriteLine(conunt.ToString() + " " + ce.key.ToString() + " Update Done....");
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                // handle the exception
                Console.WriteLine("An error occurred: " + ex.Message);
                return 0;
            }
            return 1;
        }
        //public function to create connection
        public SqlConnection Connect()
        {
            string connectionString = "Data Source=LAPTOP-JKA0T5K6\\SQLEXPRESS;Initial Catalog=CurrencyExchange;Integrated Security=True;Pooling=False";
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            return con;
        }
        // private function to create sqlCommand
        private SqlCommand CreateInsertCommand(SqlConnection con, exchangeRates CE)
        {
            var command = new SqlCommand("INSERT INTO CurrencyExchange VALUES (@key, @currentexchangerate, @currentchange, @unit, @lastupdate ,@timesample)", con);
            command.Parameters.AddWithValue("@key", CE.key);
            command.Parameters.AddWithValue("@currentexchangerate", CE.currentExchangeRate);
            command.Parameters.AddWithValue("@currentchange", CE.currentChange);
            command.Parameters.AddWithValue("@unit", CE.unit);
            command.Parameters.AddWithValue("@lastupdate", CE.lastUpdate);
            command.Parameters.AddWithValue("@timesample", DateTime.Now);
            //command.CommandType = System.Data.CommandType.StoredProcedure;
            command.ExecuteNonQuery();
            return command;
        }
        // public function to create SelectsqlCommand
        public double CreateSelectCommand(SqlConnection con) 
        {
            double CurrentExchangeRate = 0;
            var command = new SqlCommand("select CurrentExchangeRate from CurrencyExchange where [Key] = 'USD'", con);
            SqlDataReader dr2 = command.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
            while (dr2.Read())
            {
                CurrentExchangeRate = double.Parse(dr2["CurrentExchangeRate"].ToString());
            }
            con.Close();
            return CurrentExchangeRate;
        }
        private void CreateUpdateCommand(SqlConnection con, exchangeRates CE) 
        {
            var command = new SqlCommand("UPDATE CurrencyExchange SET currentExchangeRate=@currentExchangeRate,currentChange=@currentChange,unit=@unit,lastUpdate=@lastUpdate,@timesample=timesample WHERE [Key] = @key", con);
            command.Parameters.AddWithValue("@currentExchangeRate", CE.currentExchangeRate);
            command.Parameters.AddWithValue("@currentChange", CE.currentChange);
            command.Parameters.AddWithValue("@unit", CE.unit);
            command.Parameters.AddWithValue("@lastUpdate", CE.lastUpdate);
            command.Parameters.AddWithValue("@key", CE.key);
            command.Parameters.AddWithValue("@timesample", DateTime.Now);
            command.ExecuteNonQuery();
        }

    }

    // CurrencyExchange Class definition and parameters
    public class exchangeRates
    {
        public string key { get; set; }
        public double currentExchangeRate { get; set; }
        public double currentChange { get; set; }
        public int unit { get; set; }
        public DateTime lastUpdate { get; set; }

    }


    class Program
    {
        public static bool change = false;
        public static bool initializeDB = true;
        static void Main(string[] args)
        {
                // Create timer with interval of 20 minutes
                var timer = new System.Timers.Timer(20 * 60 * 1000);
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            
        }
        private static async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Stop the timer
            ((System.Timers.Timer)sender).Stop();
            // Execute function
            await MyFunctionAsync();
            // Start the timer
            ((System.Timers.Timer)sender).Start();

        }
        private static async Task MyFunctionAsync()
        {
            //initialize API string and httpClient object  
            var apiUrl = "https://boi.org.il/PublicApi/GetExchangeRates";
            var httpClient = new HttpClient();
            var modeljson = new ModelJSON();

            //Sand http get request to Bank of Israel and get response  (using async method)
            var response = await httpClient.GetAsync(apiUrl);
            var responseBody = await response.Content.ReadAsStringAsync();
            //initialize sand email function
            async Task SendEmailAsync(string resBody)
            {
                //initialize parameters for email
                string to = "jonathanmicrosoftacc@gmail.com";
                string subject = "Update Currency Exchange Rates";
                string body = resBody;
                string from = "emailsander9090@gmail.com";
                string password = "arqigtwdxbsryudc";
                MailMessage message = new MailMessage(from, to, subject, body);
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(from, password);
                try
                {
                    await client.SendMailAsync(message);
                    Console.WriteLine("Email sent successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error sending email: " + ex.Message);
                }
            }
            //Check the previous value of usd 
            double usd = Math.Round(modeljson.CreateSelectCommand(modeljson.Connect()),3);
            //convert the json object to ExchangeRate object
            modeljson = JsonConvert.DeserializeObject<ModelJSON>(responseBody);
            //if there is a change in USD and sand the right email
            if (usd != modeljson.exchangeRates[0].currentExchangeRate) 
            {
                change = true;
                string resChange = "";
                
                resChange = "New USD ExchangeRate is: "+ modeljson.exchangeRates[0].currentExchangeRate.ToString() + " Old USD ExchangeRate is:  "+usd.ToString() + " The Service was executed at " + DateTime.Now.ToString();
                await SendEmailAsync(resChange);
            }
            else
            {
                change = false;
                string resNotChange = "";
                foreach (exchangeRates ce in modeljson.exchangeRates)
                {
                    resNotChange += "Coin: " + ce.key.ToString() + " Current_Exchange_Rate: " + ce.currentExchangeRate.ToString() + " currentChange: " + ce.currentChange.ToString() + " unit: " + ce.unit.ToString() + " lastUpdate: "+ce.lastUpdate.ToString() +"\n";
                }
                resNotChange += "The Service was executed at " + DateTime.Now.ToString();
                await SendEmailAsync(resNotChange);
            }

            if(initializeDB==false)
            {
                //insert all Currency_Exchange to the SQL server
                
                
                
                
                
                
                
                
                var commit = modeljson.InsertCE();
                if (commit == 0)
                {
                    Console.WriteLine("Insert CurrencyExchange Function to the SQL server  executed at " + DateTime.Now.ToString() + " and failed");
                }
                else
                {
                    Console.WriteLine("Insert CurrencyExchange Function to the SQL server executed at " + DateTime.Now.ToString() + " Successfully");
                }
                initializeDB = true;
            }
            else 
            {
                var commit = modeljson.UpdateCE();
                if (commit == 0)
                {
                    Console.WriteLine("Update CurrencyExchange Function to the SQL server  executed at " + DateTime.Now.ToString() + " and failed");
                }
                else
                {
                    Console.WriteLine("Update CurrencyExchange Function to the SQL server executed at " + DateTime.Now.ToString() + " Successfully");
                }
            }
            
        }

    }
}

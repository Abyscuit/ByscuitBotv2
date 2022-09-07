using ByscuitBotv2.Modules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ByscuitBotv2.Data
{
    public class Nanopool
    {
        #region RESPONSES
        // ---------- CLASSES FOR RESPONSES ----------
        public class Response
        {
            public bool status { get; set; }
            public Account data { get; set; }
            public string error { get; set; }
        }
        public class WorkerResponse
        {
            public bool status { get; set; }
            public List<Worker> data { get; set; }
            public string error { get; set; }
        }
        public class Account
        {
            public string account { get; set; }
            public string unconfirmed_balance { get; set; }
            public string balance { get; set; }
            public string hashrate { get; set; }
            public Dictionary<string, string> avgHashrate { get; set; }
            public List<Worker> workers { get; set; }
        }

        public class Worker
        {
            public string id { get; set; }
            public ulong uid { get; set; }
            public string hashrate { get; set; }
            public ulong lastshare { get; set; }
            public uint rating { get; set; }
            public string h1 { get; set; }
            public string h3 { get; set; }
            public string h6 { get; set; }
            public string h12 { get; set; }
            public string h24 { get; set; }
            public uint prevShares { get; set; }
            public uint termShares { get; set; }
            public Worker()
            {
                id = "NewWorker";
                uid = 0;
                hashrate = "0";
                lastshare = 0;
                rating = 0;
                h1 = "0";
                h3 = "0";
                h6 = "0";
                h12 = "0";
                h24 = "0";
                prevShares = 0;
                termShares = 0;
            }

            public void calcTermShares()
            {
                // Calculate the rating with the workerstates instead
                WorkerStates.WorkerStateStruct WorkerStruct = WorkerStates.getWorkerStruct(this);
                Utility.printConsole($"Calculating {WorkerStruct.id} shares...");

                // If reset set prevShares to 0 and add a new state
                if (rating < prevShares)
                {
                    prevShares = 0;
                    // Only add a new state if the last share was over 2 days
                    DateTimeOffset lastShareUTC = DateTimeOffset.FromUnixTimeSeconds((long)WorkerStruct.GetLastState().lastshare);
                    double dLastShare = DateTimeOffset.UtcNow.Subtract(lastShareUTC).TotalDays;
                    Utility.printConsole($"{WorkerStruct.id} Mined: {dLastShare} Days ago");
                    if (dLastShare >= 2.5) WorkerStruct.AddNewState(this);
                }

                // Quick fix
                // If the saved prev share count is now lower than the current rating
                // and if they have more than 1 saved state set the prev share count to zero 
                if (WorkerStruct.states.Count > 1) this.prevShares = 0;
                else { // Preserve prev share count if reset
                    if (WorkerStruct.states[0].prevShares > 0) {
                        prevShares = WorkerStruct.states[0].prevShares;
                    }
                }
                WorkerStruct.ReplaceCurrentState(this); // Replace current state in case reset
                termShares = WorkerStruct.GetTotalShares();
            }
        }

        // Class for general info response
        public class GeneralInfoResponse
        {
            public bool status;
            public GeneralData data;
        }

        // Data Class for general info call
        public class GeneralData
        {
            public string balance;
            public string unconfirmed_balance;
            public string hashrate;
            public GenAvgHashrate avghashrate;
            public GenWorker[] worker;
        }

        // Worker class for general info call
        public class GenWorker
        {
            public string id { get; set; }
            public string hashrate { get; set; }
            public ulong lastShare { get; set; }
            public string avg_h1 { get; set; }
            public string avg_h3 { get; set; }
            public string avg_h6 { get; set; }
            public string avg_h12 { get; set; }
            public string avg_h24 { get; set; }
        }

        // Average hashrate class for general info call
        public class GenAvgHashrate
        {
            public string h1 { get; set; }
            public string h3 { get; set; }
            public string h6 { get; set; }
            public string h12 { get; set; }
            public string h24 { get; set; }
        }

        public class PriceResponse
        {
            public bool status;
            public Prices data;
        }
        public class Prices
        {
            public double price_usd;
            public double price_btc;
            public double price_eur;
            public double price_rur;
            public double price_CNY;
            public double price_gbp;
        }

        public class PaymentResponse
        {
            public bool status;
            public List<Payment> data;
        }
        public class Payment
        {
            public ulong date;
            public string txHash;
            public double amount;
            public bool confirmed;
        }

        public class CalculatorResponse
        {
            public bool status;
            public CalculatorData data;
        }

        public class CalculatorData
        {
            public CalculatorOutput minute;
            public CalculatorOutput hour;
            public CalculatorOutput day;
            public CalculatorOutput week;
            public CalculatorOutput month;
        }

        public class CalculatorOutput
        {
            public double coins;
            public double bitcoins;
            public double dollars;
            public double yuan;
            public double euros;
            public double rubles;

        }
        #endregion

        #region VARIABLES
        // ---------- VARIABLES ----------
        public Nanopool.Account account;// Mining account variable
        List<Nanopool.Worker> workers = new List<Nanopool.Worker>();// List of all workers
        string folderPath = Directory.GetCurrentDirectory() + "/Nanopool";
        bool calculatedTotals = false;// Check to make sure user calc totals before a save
        public static string ADDRESS = "0xe4a555daf0c71abef7b2d725eefae41dead4d8dd";
        string balance = "";
        string balValue = "";
        public string ethUSDValue = "";
        double dETHUSDValue = -1;
        public static double payoutThreshold = 0.4; // Minimum payout variable
        

        static string nanopoolGenInfo = "https://api.nanopool.org/v1/eth/user/";
        static string nanopoolWorkers = "https://api.nanopool.org/v1/eth/workers/";
        static string nanopoolPayments = "https://api.nanopool.org/v1/eth/payments/";
        static string nanopoolCalculator = "https://api.nanopool.org/v1/eth/approximated_earnings/";
        #endregion

        // ---------- FUNCTIONS ------------

        private string[] getDateFromFile(string file)
        {
            string fileName = folderPath + "/";
            string[] split = file.Remove(0, fileName.Length).Split(' ');
            string date = split.Length > 0 ? split[0] : "";
            return date.Split('-');
        }

        private void loadWorkers()
        {
            string fileName = folderPath + "/";
            string newest = "";
            foreach (string file in Directory.GetFiles(folderPath))
            {
                // Add if date is older load newest check
                if (newest == "") { newest = file; continue; }

                string[] newestDate = getDateFromFile(newest);// split the newest name into date numbers [MM, DD, YY]

                string[] fileDate = getDateFromFile(file);// split the file name into date numbers [MM, DD, YY]

                int fileYear = int.Parse(fileDate[2]);
                int newestYear = int.Parse(newestDate[2]);

                // Check if year is newer if it is replace newest with current
                if (fileYear > newestYear) { newest = file; continue; }

                int fileMonth = int.Parse(fileDate[0]);
                int newestMonth = int.Parse(newestDate[0]);

                // Check if month is newer if it is replace newest with current
                if (fileMonth > newestMonth) { newest = file; continue; }
                if (fileMonth == newestMonth)
                {
                    int fileDay = int.Parse(fileDate[1]);
                    int newestDay = int.Parse(newestDate[1]);
                    if (fileDay > newestDay) { newest = file; continue; }
                }
            }
            fileName = newest;
            string json = File.ReadAllText(fileName);
            List<Nanopool.Worker> tempWorkers = JsonConvert.DeserializeObject<List<Nanopool.Worker>>(json);
            foreach (Nanopool.Worker worker in tempWorkers)
                foreach (Nanopool.Worker currentMiner in workers)
                    if (worker.id == currentMiner.id) currentMiner.prevShares = worker.rating;// Place rating of the saved data to the previous shares

            Utility.printDEBUG("Successfully loaded file " + fileName);
        }

        public void SaveFile()
        {
            // Write crypto share file ex: "5-23-2020 07-26.json"
            string fileName = DateTime.Now.ToLocalTime().ToString("M-dd-yyyy HH-mm") + ".json";
            string file = folderPath + "/" + fileName;
            string json = JsonConvert.SerializeObject(workers, Formatting.Indented);
            File.WriteAllText(file, json);
            Utility.printConsole("Successfully saved Nanopool data");
        }

        public void FolderExist()
        {
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        }
        public void Save()
        {
            FolderExist();
            SaveFile();
        }

        void getInfo()
        {
            account = Nanopool.GetAccount(ADDRESS);
            workers = Nanopool.GetWorkers(ADDRESS);
            // Create it on its own thread
            //while (account == null) { Thread.Sleep(10); }
            balance = account.balance;
            dETHUSDValue = GetPrices().price_usd;
            ethUSDValue = string.Format("${0:N2}", dETHUSDValue);
            if (dETHUSDValue != -1) balValue = string.Format("${0:N2}", dETHUSDValue * double.Parse(balance));
        }
        private string Calculate()
        {
            if (workers == null) return "";
            double bal = double.Parse(balance);// Variable for the total current balance

            // Add miners that are not currently mining
            if (workers.Count != WorkerStates.states.Count)
            {
                List<WorkerStates.WorkerStateStruct> notMining = WorkerStates.GetMinersNotMining(workers);

                for (int i = 0; i < notMining.Count; i++)
                {
                    Worker worker = new Worker();
                    worker.id = notMining[i].id;
                    worker.uid = notMining[i].uid;
                    worker.termShares = notMining[i].GetTotalShares();
                    workers.Add(worker);
                }
            }

            // Loop through all miners to calculate total shares and miner totals
            uint totalTermShares = 0;
            foreach (Nanopool.Worker miner in workers)
            {
                // Calculate previous shares of each miner and subtract that from the
                // current amount they have to get the amount mined in the term
                // currentShares - prevShares = actualPayoutShares
                if(miner.rating != 0) miner.calcTermShares();
                totalTermShares += miner.termShares;// Add all the term shares of ALL miners (including the miner in calc)
            }
            byscuitBot.CommandHandler.WS_UPDATED_DATE = DateTime.Now;

            // Get the term shares of ALL miners and divide current shares 
            // by the total amount of shares
            // actualPayoutShares / totalPayoutShares = percentPayout
            // Total (Shares XXXX):
            // NAME XX.XX% (X.XXXXXXXX ETH | X.XXXXXXXX BNB | $XX.XX) | (Shares XX)
            // NAME XX.XX% (X.XXXXXXXX ETH | X.XXXXXXXX BNB | $XX.XX) | (Shares XX)
            //
            // TOTAL X.XXXXXXXXXX ETH (X.XXXXXXXX BNB | $XXX.XXUSD)

            // String for the output
            string output = string.Format("__**Total (Shares {0:N0}):**__\n\n", totalTermShares);

            foreach (Nanopool.Worker miner in workers)
            {
                // Output the percent of each miner
                double termPercent = miner.termShares / (float)totalTermShares;
                double ethAmount = bal * termPercent;
                double termRate = termPercent * 100;
                double termAmount = bal * termPercent;
                double termUSDVal = termAmount * dETHUSDValue;

                // Add the workers information
                output += string.Format("_**{3}**_: {0:P} ({1:N8} ETH | ${2:N2}) | ({4:N0} Shares | {5} MH/s)\n", termRate / 100, termAmount,
                    termUSDVal, miner.id, miner.termShares, miner.hashrate);
            }

            // Display total amount of ETH mined and USD value
            output += $"\n**Total:  {balance:N8} ETH ({balValue})**";

            calculatedTotals = true;
            return output;
        }

        private void GetLastPayment()
        {
            Nanopool.Payment lastPayment = Nanopool.GetPayments(ADDRESS).First();
            balance = lastPayment.amount + "";
            if (dETHUSDValue != -1) balValue = string.Format("${0:N2}", dETHUSDValue * lastPayment.amount);
        }

        public string DoPayout()
        {
            FolderExist();
            getInfo(); // Get all info for the ethereum payout
            GetLastPayment();
            if (Directory.GetFiles(folderPath).Length > 0) loadWorkers();
            string result = Calculate();

            Save();
            WorkerStates.Reset();
            return result;
        }

        public string BalanceCheck()
        {
            FolderExist();
            getInfo(); // Get all info for the ethereum payout
            if (Directory.GetFiles(folderPath).Length > 0) loadWorkers();
            return Calculate();
        }


        public static Account GetAccount(string address)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(nanopoolGenInfo + address);
            request.ContentType = "application/json; charset=utf-8";
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            string data = "";
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                data = reader.ReadToEnd();
            }

            Response r = JsonConvert.DeserializeObject<Response>(data);
            Account account = null;

            if (r.status != false) account = r.data;
            else return null;

            return account;
        }

        static string ethPrice = "https://api.nanopool.org/v1/eth/prices";
        public static Prices GetPrices()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ethPrice);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = "GET";
            request.Timeout = 5000;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.101 Safari/537.36";
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            string data = "";
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                //Console.WriteLine($"Stream Reader: {reader.}")
                data = reader.ReadToEnd();
            }

            PriceResponse r = JsonConvert.DeserializeObject<PriceResponse>(data);
            Prices prices = null;

            if (r.status != false) prices = r.data;
            else return null;

            return prices;

        }

        public static List<Worker> GetWorkers(string address)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(nanopoolWorkers + address);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = "GET";
            request.Timeout = 5000;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.101 Safari/537.36";
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            string data = "";
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                data = reader.ReadToEnd();
            }

            WorkerResponse r = JsonConvert.DeserializeObject<WorkerResponse>(data);
            List<Worker> workers = null;

            if (r.status != false) workers = r.data;
            else return new List<Worker>();

            return workers;
        }

        public static List<Payment> GetPayments(string address)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(nanopoolPayments + address);
            request.ContentType = "application/json; charset=utf-8";
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            string data = "";
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                data = reader.ReadToEnd();
            }

            PaymentResponse r = JsonConvert.DeserializeObject<PaymentResponse>(data);
            List<Payment> payments = null;

            if (r.status != false) payments = r.data;
            else return null;

            return payments;
        }

        public static DateTimeOffset GetTimeUntilPayout(string address)
        {
            TimeSpan theMerge = DateTime.Parse("09/15/2022").Subtract(DateTime.Now);
            return DateTimeOffset.Now.AddDays(theMerge.TotalDays);
        }
    }
}

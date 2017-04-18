using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TWStockWebCrawler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string GetStockIndex()
        {
            return tboStock.Text;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        async void DownloadPageAsync()
        {
            string page = "https://tw.stock.yahoo.com/q/q?s=" + GetStockIndex();

            // ... Use HttpClient.
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(page))
            using (HttpContent content = response.Content)
            {
                try
                {
                    string result = await content.ReadAsStringAsync();

                    const string pattern = @"<td\b[^>]*?>(?<V>[\s\S]*?)</\s*td>";
                    ArrayList arr = new ArrayList();
                    foreach (Match match in Regex.Matches(result, pattern, RegexOptions.IgnoreCase))
                    {
                        if (match.Groups["V"].Value.Any(c => char.IsDigit(c)))
                            arr.Add(match.Groups["V"].Value);
                    }
                    string stockPrize = arr[19].ToString();
                    string stockBrand = arr[17].ToString();
                    stockPrize = stockPrize.Replace("<b>", "").Replace("</b>", "");
                    stockBrand = GetBrandName(stockBrand);
                    Action<string> action = ReadStockPrice;
                    Invoke(action, stockBrand + "股價=" + stockPrize);
                }
                catch
                {
                    MessageBox.Show("查詢錯誤，請確定輸入的股票代碼是否正確");
                }
            }
        }
        
        private string GetBrandName(string stockBrand)
        {
            int index1 = stockBrand.IndexOf("</a>");
            int index2 = stockBrand.IndexOf(GetStockIndex());
            stockBrand = stockBrand.Substring(index2, index1 - index2);
            int index3 = stockBrand.LastIndexOf(GetStockIndex());
            stockBrand = stockBrand.Substring(index3, stockBrand.Length - index3);
            for (int i = 0; i < stockBrand.Length; i++)
            {
                if(char.IsDigit(stockBrand[i]))
            {
                    stockBrand = stockBrand.Replace(stockBrand[i].ToString(), " ");
            }
            }
            return stockBrand;
        }

        private void ReadStockPrice(string stockPrize)
        {
            labStockPrice.Text = stockPrize;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Task t = new Task(DownloadPageAsync);
            t.Start();
        }
    }
}

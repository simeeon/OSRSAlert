using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;


namespace OSRS_Alerter
{
    class Program
    {
        static void Main(string[] args)
        {
            const string URL = "https://www.playerauctions.com/rs2007-gold/?SortField=cheapest-price&Quantity={0}0&PageSize=30&PageIndex=1";
            const decimal DesiredPrice = 8.5m;
            const int MinimumMinutes = 3;
            const int MaximumMinutes = 6;
            const string Selector = ".offer-price";
            const int MaxQuantity = 10; //10-100M.
            const string SoundPath = "alert_sound.wav";

            var parser = new HtmlParser();
            var webClient = new WebClient();

            while (true)
            {
                for (int i = 1; i <= MaxQuantity; i++)
                {
                    var minPrice = GetMinPrice(webClient, parser, Selector, URL, i);

                    if (minPrice == 0)
                    {
                        continue;
                    }

                    LogPrice(i, minPrice);

                    if ((minPrice / i) <= DesiredPrice)
                    {
                        Console.WriteLine("^ ^ ^ WINNER WINNER CHICKEN DINNER ^ ^ ^");
                        PlaySound(SoundPath);
                    }

                    Thread.Sleep(new Random().Next(5000, 30000));
                }
                Console.WriteLine(new String('-', 50));

                Thread.Sleep(new Random().Next(MinimumMinutes * 60000, MaximumMinutes * 60000));
            }
        }

        private static void LogPrice(int i, decimal minumumPrice)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"{DateTime.Now} -> ");

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write($"{i}0");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" M. ");

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write($"$ {(minumumPrice / i):F2} ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("/ 10 M.");
        }

        public static void PlaySound(string file)
        {
            Process.Start(@"powershell", $@"-c (New-Object Media.SoundPlayer '{file}').PlaySync();");
        }

        private static decimal GetMinPrice(WebClient webClient, HtmlParser parser, string Selector, string URL, int i)
        {
            string html = null;

            for (int j = 0; j < 10; j++)
            {
                try
                {
                    html = webClient.DownloadString(string.Format(URL, i));
                    break;
                }
                catch (Exception e)
                {
                    Thread.Sleep(10000);
                }
            }

            if (string.IsNullOrWhiteSpace(html))
            {
                return 0;
            }

            var document = parser.Parse(html);
            var cells = document.QuerySelectorAll(Selector);

            var prices = new List<decimal>();

            foreach (var cell in cells)
            {
                var cellContent = cell?.TextContent?.Trim();
                var startIndex = cellContent.IndexOf('$');
                var endIndex = cellContent.LastIndexOf('$');

                if (startIndex >= 0)
                {
                    var relevantCellContent = cellContent.Substring(startIndex + 2, 5);
                    decimal price;
                    decimal.TryParse(relevantCellContent, out price);
                    prices.Add(price);
                }
            }
            return prices.Min();
        }
    }
}

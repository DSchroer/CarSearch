using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using CarSearch.Email;
using CarSearch.Models;
using CarSearch.Scrapers;
using CarSearch.Tools;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace CarSearch
{
    class Program
    {
        static List<Scraper<Car>> Scrapers = new List<Scraper<Car>>();
        static Notifier Notifier = new Notifier();
        static HashSet<string> Filter = new HashSet<string>();

        static void Main(string[] args)
        {
            Scrapers.Add(new HGregoire("volkswagen", null));
            Scrapers.Add(new HGregoire("subaru", null));
            Scrapers.Add(new VWCentre());
            Scrapers.Add(new VWCAndC());
            Scrapers.Add(new VWDesSources());

            Run();
            Schedule(60);

            while (true)
            {
                Thread.Sleep(10 * 1000);
            }
        }

        static void Test<T>(Scraper<T> scraper)
        {
            scraper.Execute();
            Console.WriteLine(JsonConvert.SerializeObject(scraper.Results, Formatting.Indented));
        }

        static void Schedule(int minutes)
        {
            var aTimer = new System.Timers.Timer(minutes * 60 * 1000);
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Start();
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                Run();
            }
            catch (Exception err)
            {
                Console.Error.WriteLine(err.Message);
            }
        }

        static void Run()
        {
            Console.WriteLine($"Running search at {DateTime.Now.ToString()}.");

            var cars = RunSearch();

            cars = cars.OrderBy(a => a.Price);
            cars = cars.Where(c => c.Price < 10000);
            cars = cars.Where(c => c.Transmission == Transmission.Manual);

            cars = cars.Where(c => !Filter.Contains(c.CarHash()));

            var carList = cars.ToList();
            Console.WriteLine($"Search complete found {carList.Count()} cars.");

            Notifier.Send(carList);
            foreach (var car in carList)
            {
                Filter.Add(car.CarHash());
            }
        }

        static IEnumerable<Car> RunSearch()
        {
            Scrapers.AsParallel().ForAll(s => s.Execute());
            return Scrapers.SelectMany(s => s.Results);
        }
    }
}

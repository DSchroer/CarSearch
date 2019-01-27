
using System;
using System.Linq;
using System.Text.RegularExpressions;
using CarSearch.Models;

namespace CarSearch.Scrapers
{
    class HGregoire : Scraper<Car>
    {
        public HGregoire(string make, string model)
        {
            if (model != null)
            {
                make = $"{make}-{model}";
            }
            Url = $"https://www.hgregoire.com/used-car/{make}?price_sort=1";

            Setup();
        }

        private void Setup()
        {
            Transform("details", xp => xp.Child().WithClass("car-details"))
                .Constructs(() => new Car())
                .Build();

            Transform("details", "name", xp => xp.Child().WithAttribute("itemprop", "name"))
                .SetsValue((c, n) => c.Name = n.InnerText.Trim().Split('\n').First())
                .Build();

            Transform("details", "transmission", xp => xp.Child().WithAttribute("itemprop", "vehicleTransmission"))
                .SetsValue((c, n) => c.Transmission = ParseTransmission(n.InnerText))
                .Build();

            Transform("details", "price", xp => xp.Child().WithAttribute("itemprop", "offers"))
                .SetsValue((c, n) => c.Price = ParsePrice(n.InnerText))
                .Build();

            Transform("details", "url", xp => xp.Child().WithAttribute("itemprop", "url"))
                .SetsValue((c, n) => c.Url = n.GetAttributeValue("href", ""))
                .Build();

            Transform("details", "odometer", xp => xp.Child().WithAttribute("itemprop", "mileageFromOdometer")).Build();
            Transform("odometer", "mileage", xp => xp.Child().WithAttribute("itemprop", "value"))
                .SetsValue((c, n) => c.Mileage = (int)InnerNumber(n))
                .Build();
        }

        private Transmission ParseTransmission(string value)
        {
            var match = new Regex(@"Transmission: (\w+)");
            var result = match.Match(value).Groups[1];
            if (result != null && Enum.TryParse<Transmission>(result.Value, true, out Transmission transmission))
            {
                return transmission;
            }

            return Transmission.Unknown;
        }

        private decimal ParsePrice(string value)
        {
            var match = new Regex(@"\$(\d+\ ?\d+)");
            var result = match.Match(value).Groups[1];
            if (result != null && decimal.TryParse(result.Value.Replace(" ", ""), out decimal price))
            {
                return price;
            }

            return 0;
        }
    }
}
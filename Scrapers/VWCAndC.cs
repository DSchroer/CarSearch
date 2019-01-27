
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CarSearch.Models;

namespace CarSearch.Scrapers
{
    class VWCAndC : Scraper<Car>
    {
        public VWCAndC()
        {
            Url = "https://www.campbellandcameron.ca/occasion/recherche.html";

            Setup();
        }

        private void Setup()
        {
            Transform("car", c => c.Child().WithClass("main_picbox"))
                .Constructs(() => new Car())
                .Build();

            Transform("car", "body", c => c.Child().WithClass("text_box2")).Build();

            Transform("body", "name", c => c.DirectChild("div", "h2", "a"))
                .SetsValue((c, n) => c.Name = n.InnerText.Trim().Replace(" � Lasalle, Montr&eacute;al", ""))
                .Build();

            Transform("body", "url", c => c.Child("a"))
                .SetsValue((c, n) => c.Url = n.GetAttributeValue("href", ""))
                .Build();

            Transform("body", "transmission", c => c.DirectChild("div").Index(2))
                .SetsValue((c, n) => c.Transmission = TransmissionMap(n.InnerText.Trim()))
                .Build();

            Transform("car", "details", c => c.Child().WithClass("details_box")).Build();

            Transform("details", "price", c => c.Child().WithClass("box2_details_text").DirectChild("div", "div"))
                .SetsValue((c, n) => c.Price = InnerNumber(n))
                .Build();

            Transform("details", "mileage", c => c.Child().WithClass("box2_details_text").Child("span"))
                .SetsValue((c, n) => c.Mileage = (int)InnerNumber(n))
                .Build();
        }

        protected override IEnumerable<Car> Filter(IEnumerable<Car> results)
        {
            return results.Where(c => c.Name != null);
        }

        private Transmission TransmissionMap(string value)
        {
            if (value.Contains("Automatique"))
            {
                return Transmission.Automatic;
            }

            if (value.Contains("Manuelle"))
            {
                return Transmission.Manual;
            }

            return Transmission.Unknown;
        }

    }
}
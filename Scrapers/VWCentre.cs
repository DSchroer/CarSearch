
using System;
using System.Linq;
using System.Text.RegularExpressions;
using CarSearch.Models;

namespace CarSearch.Scrapers
{
    class VWCentre : Scraper<Car>
    {
        private string Base = "http://www.vwcentreville.com";

        public VWCentre()
        {
            Url = Base + "/fr/a-vendre/auto/occasion";

            Setup();
        }

        private void Setup()
        {
            Transform("details", c => c.Child("article"))
                .Constructs(() => new Car())
                .Build();

            Transform("details", "price", c => c.Child().WithClass("preview-price"))
                .SetsValue((c, n) => c.Price = InnerNumber(n))
                .Build();

            Transform("details", "name", c => c.Child().WithClass("preview-name"))
                .SetsValue((c, n) => c.Name = n.InnerText.Trim())
                .Build();

            Transform("details", "url", c => c.Child().WithClass("preview-name"))
                .SetsValue((c, n) => c.Url = Base + n.GetAttributeValue("href", ""))
                .Build();

            Transform("details", "transmission", c => c.Child().WithClass("preview-transmission"))
                .SetsValue((c, n) => c.Transmission = TransmissionMap(n.InnerText.Trim()))
                .Build();

            Transform("details", "mileage", c => c.Child().WithClass("preview-km"))
                .SetsValue((c, n) => c.Mileage = (int)InnerNumber(n))
                .Build();
        }

        private Transmission TransmissionMap(string value)
        {
            switch (value)
            {
                case "Automatique":
                    return Transmission.Automatic;
                case "Manuelle":
                    return Transmission.Manual;
                default:
                    return Transmission.Unknown;
            }
        }

    }
}
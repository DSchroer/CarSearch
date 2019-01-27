
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using CarSearch.Models;
using HtmlAgilityPack;

namespace CarSearch.Scrapers
{
    class VWDesSources : Scraper<Car>
    {
        private string Base = "https://volkswagendessources.ca";
        private string Search = "/used-inventory/index.htm";

        public VWDesSources()
        {
            Url = Base + Search + "?internetPrice=-15000";

            Setup();
        }

        private void Setup()
        {
            Transform("nextLink", c => c.Child("a").WithAttribute("rel", "next"))
                .Follows(n => Base + Search + HttpUtility.HtmlDecode(n.GetAttributeValue("href", "")))
                .Build();

            Transform("details", c => c.Child().WithClass("hproduct").WithClass("auto"))
                .Constructs(() => new Car())
                .Build();

            Transform("details", "allValues", c => c.Child("dl"))
                .SetsValue(DataList)
                .Build();

            Transform("details", "name", c => c.Child("h3"))
                .SetsValue((c, n) => c.Name = n.InnerText.Trim())
                .Build();

            Transform("details", "url", c => c.Child("a").WithClass("view-link"))
                .SetsValue((c, n) => c.Url = Base + n.GetAttributeValue("href", ""))
                .Build();

            Transform("details", "price", c => c.Child().WithClass("final-price").Child().WithClass("value"))
                .SetsValue((c, n) => c.Price = InnerNumber(n))
                .Build();
        }

        private void DataList(Car car, HtmlNode node)
        {
            var data = new Dictionary<string, string>();
            var children = node.ChildNodes;
            for (var i = 0; i < children.Count; i++)
            {
                if (children[i].Name != "dt")
                {
                    continue;
                }

                var current = children[i];
                var sibling = current.NextSibling;
                while (sibling != null)
                {
                    if (sibling.Name == "dd")
                    {
                        data[current.InnerText] = sibling.InnerText;
                        break;
                    }

                    sibling = sibling.NextSibling;
                }
            }

            if (data.ContainsKey("Transmission:"))
            {
                car.Transmission = TransmissionMap(data["Transmission:"]);
            }

            if (data.ContainsKey("Kilométerage:"))
            {
                car.Mileage = (int)InnerNumber(data["Kilométerage:"]);
            }
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
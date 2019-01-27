
namespace CarSearch.Models
{
    class Car
    {
        public string Name { get; set; }
        public Transmission Transmission { get; set; }
        public decimal Price { get; set; }
        public int Mileage { get; set; }
        public string Url { get; set; }

        public string CarHash()
        {
            return Url;
        }
    }

    enum Transmission
    {
        Unknown,
        Manual,
        Automatic
    }
}
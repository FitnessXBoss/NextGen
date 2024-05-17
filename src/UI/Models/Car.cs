namespace NextGen.src.Data.Database.Models
{
    public class Car
    {
        public int CarId { get; set; }
        public int TrimId { get; set; }
        public string? ImageUrl { get; set; }
        public string? AdditionalFeatures { get; set; }
        public string? Status { get; set; }
        public int Year { get; set; }
        public string? Color { get; set; }
    }

    public class Brand
    {
        public int BrandId { get; set; }
        public string? BrandName { get; set; }
        public string? BrandIconUrl { get; set; }
    }

    public class Model
    {
        public int ModelId { get; set; }
        public int BrandId { get; set; }
        public string? ModelName { get; set; }
        public string? ModelImageUrl { get; set; }
    }

    public class Trim
    {
        public int TrimId { get; set; }
        public int ModelId { get; set; }
        public string? TrimName { get; set; }
        public string? TrimDetails { get; set; }
        public decimal Price { get; set; }
    }

    public class CarSummary
    {
        public int ModelId { get; set; }
        public string? ModelName { get; set; }
        public decimal MinPrice { get; set; }
        public string? ModelImageUrl { get; set; }
        public string? BrandName { get; set; }
        public string? BrandIconUrl { get; set; }
        public int CarCount { get; set; }
        public int ColorCount { get; set; }
    }

    public class CarWithTrimDetails : Car
    {
        public string? TrimName { get; set; }
        public string? TrimDetails { get; set; }
        public decimal Price { get; set; }
        public string? Transmission { get; set; }
        public string? Drive { get; set; }
        public string? Fuel { get; set; }
        public string? EngineVolume { get; set; }
        public string? HorsePower { get; set; }
    }
}

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
        public string? BrandName { get; set; } 
        public string? ModelName { get; set; } 
        public string? BrandIconUrl { get; set;}
    }

    public class CarDetails
    {
        public int CarId { get; set; }
        public int TrimId { get; set; }
        public string? ImageUrl { get; set; }
        public string? Status { get; set; }
        public string? Color { get; set; }
        public string? AdditionalFeatures { get; set; }
        public int Year { get; set; }
        public List<CarImage> Images { get; set; }
        public string? Transmission { get; set; }
        public string? Drive { get; set; }
        public string? Fuel { get; set; }
        public string? EngineVolume { get; set; }
        public string? HorsePower { get; set; }
        public decimal Price { get; set; }
        public string? TrimDetails { get; set; }
        public int Seats { get; set; }
        public string? Length { get; set; }
        public string? Width { get; set; }
        public string? Height { get; set; }
        public string? TrunkVolume { get; set; }
        public string? FuelTankVolume { get; set; }
        public string? MixedConsumption { get; set; }
        public string? CityConsumption { get; set; }
        public string? HighwayConsumption { get; set; }
        public string? MaxSpeed { get; set; }
        public string? Acceleration { get; set; }
        public string? CarBodyType { get; set; }

    }

    public class CarImage
    {
        public int ImageId { get; set; }
        public string? ImagePath { get; set; }
        public string? Description { get; set; }
    }


}

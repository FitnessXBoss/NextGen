namespace NextGen.src.Data.Database.Models
{
    public class Car
    {
        public int CarId { get; set; }
        public string? Model { get; set; }
        public string? Status { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? BrandName { get; set; }  // Название бренда
        public string? BrandIconUrl { get; set; }  // URL иконки бренда
        public string? TrimName { get; set; }  // Название комплектации
        public string? TrimDetails { get; set; }  // Описание комплектации\
        public int CarCount { get; set; }
        public int ColorCount { get; set; }
    }

    public class Brand
    {
        public int BrandId { get; set; }
        public string? BrandName { get; set; }
    }

    public class Model
    {
        public int ModelId { get; set; }
        public int BrandId { get; set; }
        public string? ModelName { get; set; }
        public string? ModelImageUrl { get; set; }
    }
}

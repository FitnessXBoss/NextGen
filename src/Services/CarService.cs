using Npgsql;
using NextGen.src.Data.Database.Models;
using System.Collections.Generic;
using System.Configuration;
using NextGen.src.UI.ViewModels;


namespace NextGen.src.Services
{
    public class CarService
    {
        private readonly string connectionString;

        public CarService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["SecurityData"].ConnectionString;
        }

        public IEnumerable<Car> GetAllCars()
        {
            var cars = new List<Car>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand(@"SELECT m.model_id, m.model_name, COALESCE(MIN(t.price), 0) as min_price, m.model_image_url,
                    b.brand_name, b.brand_icon_url, 
                    COALESCE(COUNT(DISTINCT c.car_id), 0) as car_count,
                    COALESCE(COUNT(DISTINCT c.color), 0) as color_count
                FROM models m
                LEFT JOIN trims t ON m.model_id = t.model_id
                LEFT JOIN cars c ON t.trim_id = c.trim_id
                LEFT JOIN brands b ON m.brand_id = b.brand_id
                GROUP BY m.model_id, m.model_name, m.model_image_url, b.brand_name, b.brand_icon_url", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cars.Add(new Car
                        {
                            ModelId = reader.GetInt32(reader.GetOrdinal("model_id")),
                            Model = reader.GetString(reader.GetOrdinal("model_name")),
                            Price = reader.IsDBNull(reader.GetOrdinal("min_price")) ? 0 : reader.GetDecimal(reader.GetOrdinal("min_price")),
                            ImageUrl = reader.IsDBNull(reader.GetOrdinal("model_image_url")) ? null : reader.GetString(reader.GetOrdinal("model_image_url")),
                            BrandName = reader.GetString(reader.GetOrdinal("brand_name")),
                            BrandIconUrl = reader.IsDBNull(reader.GetOrdinal("brand_icon_url")) ? null : reader.GetString(reader.GetOrdinal("brand_icon_url")),
                            CarCount = reader.GetInt32(reader.GetOrdinal("car_count")),
                            ColorCount = reader.GetInt32(reader.GetOrdinal("color_count"))
                        });
                    }
                }
            }
            return cars;
        }

        public IEnumerable<Model> GetModelsByBrand(int brandId)
        {
            var models = new List<Model>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("SELECT model_id, model_name, model_image_url FROM models WHERE brand_id = @brandId", connection);
                cmd.Parameters.AddWithValue("@brandId", brandId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        models.Add(new Model
                        {
                            ModelId = reader.GetInt32(reader.GetOrdinal("model_id")),
                            ModelName = reader.GetString(reader.GetOrdinal("model_name")),
                            ModelImageUrl = reader.GetString(reader.GetOrdinal("model_image_url")),
                            BrandId = brandId
                        });
                    }
                }
            }
            return models;
        }


        public IEnumerable<Brand> GetAllBrands()
        {
            var brands = new List<Brand>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("SELECT brand_id, brand_name FROM brands", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        brands.Add(new Brand
                        {
                            BrandId = reader.GetInt32(reader.GetOrdinal("brand_id")),
                            BrandName = reader.GetString(reader.GetOrdinal("brand_name")),
                        });
                    }
                }
            }
            return brands;
        }




    }
}

using Npgsql;
using NextGen.src.Data.Database.Models;
using System.Collections.Generic;
using System.Configuration;

namespace NextGen.src.Services
{
    public class CarService
    {
        private readonly string connectionString;

        public CarService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["SecurityData"].ConnectionString;
        }

        public IEnumerable<CarSummary> GetAllCarSummaries()
        {
            var carSummaries = new List<CarSummary>();
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
                        carSummaries.Add(new CarSummary
                        {
                            ModelId = reader.GetInt32(reader.GetOrdinal("model_id")),
                            ModelName = reader.GetString(reader.GetOrdinal("model_name")),
                            MinPrice = reader.IsDBNull(reader.GetOrdinal("min_price")) ? 0 : reader.GetDecimal(reader.GetOrdinal("min_price")),
                            ModelImageUrl = reader.IsDBNull(reader.GetOrdinal("model_image_url")) ? null : reader.GetString(reader.GetOrdinal("model_image_url")),
                            BrandName = reader.GetString(reader.GetOrdinal("brand_name")),
                            BrandIconUrl = reader.IsDBNull(reader.GetOrdinal("brand_icon_url")) ? null : reader.GetString(reader.GetOrdinal("brand_icon_url")),
                            CarCount = reader.GetInt32(reader.GetOrdinal("car_count")),
                            ColorCount = reader.GetInt32(reader.GetOrdinal("color_count"))
                        });
                    }
                }
            }
            return carSummaries;
        }

        public IEnumerable<Model> GetModelsByBrand(int brandId)
        {
            var models = new List<Model>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("SELECT model_id, model_name, model_image_url, brand_id FROM models WHERE brand_id = @brandId", connection);
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
                            BrandId = reader.GetInt32(reader.GetOrdinal("brand_id"))
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
                var cmd = new NpgsqlCommand("SELECT brand_id, brand_name, brand_icon_url FROM brands", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        brands.Add(new Brand
                        {
                            BrandId = reader.GetInt32(reader.GetOrdinal("brand_id")),
                            BrandName = reader.GetString(reader.GetOrdinal("brand_name")),
                            BrandIconUrl = reader.IsDBNull(reader.GetOrdinal("brand_icon_url")) ? null : reader.GetString(reader.GetOrdinal("brand_icon_url"))
                        });
                    }
                }
            }
            return brands;
        }

        public IEnumerable<Trim> GetTrimsByModel(int modelId)
        {
            var trims = new List<Trim>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("SELECT trim_id, model_id, trim_name, trim_details, price FROM trims WHERE model_id = @modelId", connection);
                cmd.Parameters.AddWithValue("@modelId", modelId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        trims.Add(new Trim
                        {
                            TrimId = reader.GetInt32(reader.GetOrdinal("trim_id")),
                            ModelId = reader.GetInt32(reader.GetOrdinal("model_id")),
                            TrimName = reader.GetString(reader.GetOrdinal("trim_name")),
                            TrimDetails = reader.GetString(reader.GetOrdinal("trim_details")),
                            Price = reader.GetDecimal(reader.GetOrdinal("price"))
                        });
                    }
                }
            }
            return trims;
        }

        public IEnumerable<CarWithTrimDetails> GetCarsByTrims(List<int> trimIds)
        {
            var cars = new List<CarWithTrimDetails>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand(@"
            SELECT c.car_id, c.trim_id, c.image_url, c.additional_features, c.status, c.year, c.color,
                   t.trim_name, t.trim_details, t.price,
                   COALESCE(t.transmission, 'Не указано') AS transmission,
                   COALESCE(t.drive, 'Не указано') AS drive,
                   COALESCE(t.fuel, 'Не указано') AS fuel,
                   COALESCE(t.engine_volume, 'Не указано') AS engine_volume,
                   COALESCE(t.horse_power, 'Не указано') AS horse_power
            FROM cars c
            LEFT JOIN trims t ON c.trim_id = t.trim_id
            WHERE c.trim_id = ANY(@trimIds)", connection);
                cmd.Parameters.AddWithValue("@trimIds", trimIds);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cars.Add(new CarWithTrimDetails
                        {
                            CarId = reader.GetInt32(reader.GetOrdinal("car_id")),
                            TrimId = reader.GetInt32(reader.GetOrdinal("trim_id")),
                            ImageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? null : reader.GetString(reader.GetOrdinal("image_url")),
                            AdditionalFeatures = reader.IsDBNull(reader.GetOrdinal("additional_features")) ? null : reader.GetString(reader.GetOrdinal("additional_features")),
                            Status = reader.IsDBNull(reader.GetOrdinal("status")) ? null : reader.GetString(reader.GetOrdinal("status")),
                            Year = reader.GetInt32(reader.GetOrdinal("year")),
                            Color = reader.IsDBNull(reader.GetOrdinal("color")) ? null : reader.GetString(reader.GetOrdinal("color")),
                            TrimName = reader.GetString(reader.GetOrdinal("trim_name")),
                            TrimDetails = reader.GetString(reader.GetOrdinal("trim_details")),
                            Price = reader.GetDecimal(reader.GetOrdinal("price")),
                            Transmission = reader.GetString(reader.GetOrdinal("transmission")),
                            Drive = reader.GetString(reader.GetOrdinal("drive")),
                            Fuel = reader.GetString(reader.GetOrdinal("fuel")),
                            EngineVolume = reader.GetString(reader.GetOrdinal("engine_volume")),
                            HorsePower = reader.GetString(reader.GetOrdinal("horse_power"))
                        });
                    }
                }
            }
            return cars;
        }

    }
}

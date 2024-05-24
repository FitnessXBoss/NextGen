using Npgsql;
using NextGen.src.Data.Database.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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

        public CarDetails GetCarDetails(int carId)
        {
            CarDetails details = new CarDetails();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Получение основных данных о машине
                var carCmd = new NpgsqlCommand($@"
                    SELECT car_id, image_url, trim_id, status, color, additional_features, year 
                    FROM cars 
                    WHERE car_id = @carId", connection);
                carCmd.Parameters.AddWithValue("@carId", carId);

                using (var reader = carCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        details.CarId = reader.GetInt32(reader.GetOrdinal("car_id"));
                        details.ImageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? null : reader.GetString(reader.GetOrdinal("image_url"));
                        details.TrimId = reader.GetInt32(reader.GetOrdinal("trim_id"));
                        details.Status = reader.IsDBNull(reader.GetOrdinal("status")) ? null : reader.GetString(reader.GetOrdinal("status"));
                        details.Color = reader.IsDBNull(reader.GetOrdinal("color")) ? null : reader.GetString(reader.GetOrdinal("color"));
                        details.AdditionalFeatures = reader.IsDBNull(reader.GetOrdinal("additional_features")) ? null : reader.GetString(reader.GetOrdinal("additional_features"));
                        details.Year = reader.GetInt32(reader.GetOrdinal("year"));
                    }
                }

                // Получение дополнительных изображений
                if (connection.State == System.Data.ConnectionState.Closed)
                    connection.Open();

                var imagesCmd = new NpgsqlCommand($@"
                    SELECT image_id, image_url, description 
                    FROM car_images 
                    WHERE car_id = @carId", connection);
                imagesCmd.Parameters.AddWithValue("@carId", carId);

                using (var reader = imagesCmd.ExecuteReader())
                {
                    details.Images = new List<CarImage>();
                    while (reader.Read())
                    {
                        details.Images.Add(new CarImage
                        {
                            ImageId = reader.GetInt32(reader.GetOrdinal("image_id")),
                            ImagePath = reader.GetString(reader.GetOrdinal("image_url")),
                            Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description"))
                        });
                    }
                }
            }

            return details;
        }


        public IEnumerable<CarSummary> GetAllCarSummaries()
        {
            var carSummaries = new List<CarSummary>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand(@"
                    SELECT m.model_id, m.model_name, COALESCE(MIN(t.price), 0) as min_price, m.model_image_url,
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
                        Debug.WriteLine($"CarSummary: ModelId={reader.GetInt32(reader.GetOrdinal("model_id"))}, ModelName={reader.GetString(reader.GetOrdinal("model_name"))}");
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
                        var model = new Model
                        {
                            ModelId = reader.GetInt32(reader.GetOrdinal("model_id")),
                            ModelName = reader.GetString(reader.GetOrdinal("model_name")),
                            ModelImageUrl = reader.GetString(reader.GetOrdinal("model_image_url")),
                            BrandId = reader.GetInt32(reader.GetOrdinal("brand_id"))
                        };
                        models.Add(model);
                        Debug.WriteLine($"Model: ModelId={model.ModelId}, ModelName={model.ModelName}, BrandId={model.BrandId}");
                    }
                }
            }
            Debug.WriteLine($"Total models loaded: {models.Count}");
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
                        Debug.WriteLine($"Brand: BrandId={reader.GetInt32(reader.GetOrdinal("brand_id"))}, BrandName={reader.GetString(reader.GetOrdinal("brand_name"))}");
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
                        Debug.WriteLine($"Trim: TrimId={reader.GetInt32(reader.GetOrdinal("trim_id"))}, TrimName={reader.GetString(reader.GetOrdinal("trim_name"))}");
                    }
                }
            }
            return trims;
        }

        public IEnumerable<CarWithTrimDetails> GetCarsByTrimId(int trimId)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand($"SELECT * FROM cars WHERE trim_id = {trimId}", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return new CarWithTrimDetails
                        {
                            CarId = (int)reader["car_id"],
                            TrimId = (int)reader["trim_id"],
                            // Дополните другими свойствами, специфичными для CarWithTrimDetails
                        };
                    }
                }
            }
        }



        public IEnumerable<CarWithTrimDetails> GetCarsByTrims(List<int> trimIds)
        {
            var cars = new List<CarWithTrimDetails>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand(@"SELECT c.car_id, c.trim_id, 
       COALESCE(c.image_url, 'https://i.ibb.co/hsPFKrr/free-icon-page-not-found-4380687.png') as image_url,
       c.additional_features, c.status, c.year, 
       COALESCE(c.color, 'Unknown') as color,
       t.trim_name, t.trim_details, t.price,
       COALESCE(t.transmission, 'Не указано') AS transmission,
       COALESCE(t.drive, 'Не указано') AS drive,
       COALESCE(t.fuel, 'Не указано') AS fuel,
       COALESCE(t.engine_volume, 'Не указано') AS engine_volume,
       COALESCE(t.horse_power, 'Не указано') AS horse_power,
       m.model_name, b.brand_name, b.brand_icon_url
FROM cars c
LEFT JOIN trims t ON c.trim_id = t.trim_id
LEFT JOIN models m ON t.model_id = m.model_id
LEFT JOIN brands b ON m.brand_id = b.brand_id
WHERE c.trim_id = ANY(@trimIds::integer[])", connection);
                cmd.Parameters.AddWithValue("@trimIds", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer, trimIds.ToArray());

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cars.Add(new CarWithTrimDetails
                        {
                            CarId = reader.GetInt32(reader.GetOrdinal("car_id")),
                            TrimId = reader.GetInt32(reader.GetOrdinal("trim_id")),
                            ImageUrl = reader.GetString(reader.GetOrdinal("image_url")),
                            AdditionalFeatures = reader.IsDBNull(reader.GetOrdinal("additional_features")) ? null : reader.GetString(reader.GetOrdinal("additional_features")),
                            Status = reader.IsDBNull(reader.GetOrdinal("status")) ? null : reader.GetString(reader.GetOrdinal("status")),
                            Year = reader.GetInt32(reader.GetOrdinal("year")),
                            Color = reader.GetString(reader.GetOrdinal("color")),
                            TrimName = reader.GetString(reader.GetOrdinal("trim_name")),
                            TrimDetails = reader.GetString(reader.GetOrdinal("trim_details")),
                            Price = reader.GetDecimal(reader.GetOrdinal("price")),
                            Transmission = reader.GetString(reader.GetOrdinal("transmission")),
                            Drive = reader.GetString(reader.GetOrdinal("drive")),
                            Fuel = reader.GetString(reader.GetOrdinal("fuel")),
                            EngineVolume = reader.GetString(reader.GetOrdinal("engine_volume")),
                            HorsePower = reader.GetString(reader.GetOrdinal("horse_power")),
                            BrandName = reader.GetString(reader.GetOrdinal("brand_name")),
                            ModelName = reader.GetString(reader.GetOrdinal("model_name")),
                            BrandIconUrl = reader.IsDBNull(reader.GetOrdinal("brand_icon_url")) ? null : reader.GetString(reader.GetOrdinal("brand_icon_url"))  // Извлечение URL иконки бренда
                        });
                    }
                }
            }
            return cars;
        }

    }
}

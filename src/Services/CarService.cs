﻿using Npgsql;
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
                Debug.WriteLine($"Executing SQL queries for carId: {carId}");

                // Получение основных данных о машине
                var carCmd = new NpgsqlCommand($@"
            SELECT c.car_id, c.image_url, c.trim_id, c.status, c.color, c.additional_features, c.year, c.vin, cc.color_name, cc.color_hex, t.trim_name
            FROM cars c
            LEFT JOIN car_colors cc ON c.color_id = cc.color_id
            LEFT JOIN trims t ON c.trim_id = t.trim_id
            WHERE c.car_id = @carId", connection);
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
                        details.VIN = reader.GetString(reader.GetOrdinal("vin"));
                        details.Year = reader.GetInt32(reader.GetOrdinal("year"));
                        details.ColorHex = reader.IsDBNull(reader.GetOrdinal("color_hex")) ? null : reader.GetString(reader.GetOrdinal("color_hex"));
                        details.ColorName = reader.IsDBNull(reader.GetOrdinal("color_name")) ? null : reader.GetString(reader.GetOrdinal("color_name"));
                        details.TrimName = reader.IsDBNull(reader.GetOrdinal("trim_name")) ? null : reader.GetString(reader.GetOrdinal("trim_name"));

                        Debug.WriteLine($"Loaded Car Details in GetCarDetails: CarId={details.CarId}, VIN={details.VIN}, Year={details.Year}, Color={details.Color}, TrimName={details.TrimName}, Status={details.Status}");
                    }
                    else
                    {
                        Debug.WriteLine("No car details found for given carId.");
                    }
                }

                // Получение дополнительных изображений
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

                // Получение дополнительных характеристик
                var trimCmd = new NpgsqlCommand($@"
            SELECT t.transmission, t.drive, t.fuel, t.engine_volume, t.horse_power, t.price, 
                   t.trim_name, t.trim_details, m.model_name, b.brand_name
            FROM trims t
            JOIN models m ON t.model_id = m.model_id
            JOIN brands b ON m.brand_id = b.brand_id
            WHERE t.trim_id = @trimId", connection);
                trimCmd.Parameters.AddWithValue("@trimId", details.TrimId);

                using (var reader = trimCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        details.Transmission = reader.IsDBNull(reader.GetOrdinal("transmission")) ? null : reader.GetString(reader.GetOrdinal("transmission"));
                        details.Drive = reader.IsDBNull(reader.GetOrdinal("drive")) ? null : reader.GetString(reader.GetOrdinal("drive"));
                        details.Fuel = reader.IsDBNull(reader.GetOrdinal("fuel")) ? null : reader.GetString(reader.GetOrdinal("fuel"));
                        details.EngineVolume = reader.IsDBNull(reader.GetOrdinal("engine_volume")) ? null : reader.GetString(reader.GetOrdinal("engine_volume"));
                        details.HorsePower = reader.IsDBNull(reader.GetOrdinal("horse_power")) ? null : reader.GetString(reader.GetOrdinal("horse_power"));
                        details.Price = reader.GetDecimal(reader.GetOrdinal("price"));
                        details.TrimName = reader.GetString(reader.GetOrdinal("trim_name"));
                        details.TrimDetails = reader.GetString(reader.GetOrdinal("trim_details"));
                        details.ModelName = reader.GetString(reader.GetOrdinal("model_name"));
                        details.BrandName = reader.GetString(reader.GetOrdinal("brand_name"));
                    }
                }

                // Получение характеристик из таблицы car_specs
                var specsCmd = new NpgsqlCommand($@"
            SELECT seats, length, width, height, trunk_volume, fuel_tank_volume, mixed_consumption, city_consumption, highway_consumption, max_speed, acceleration, body_type
            FROM car_specs 
            WHERE car_id = @carId", connection);
                specsCmd.Parameters.AddWithValue("@carId", carId);

                using (var reader = specsCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        details.Seats = reader.GetInt32(reader.GetOrdinal("seats"));
                        details.Length = reader.GetString(reader.GetOrdinal("length"));
                        details.Width = reader.GetString(reader.GetOrdinal("width"));
                        details.Height = reader.GetString(reader.GetOrdinal("height"));
                        details.TrunkVolume = reader.GetString(reader.GetOrdinal("trunk_volume"));
                        details.FuelTankVolume = reader.GetString(reader.GetOrdinal("fuel_tank_volume"));
                        details.MixedConsumption = reader.GetString(reader.GetOrdinal("mixed_consumption"));
                        details.CityConsumption = reader.GetString(reader.GetOrdinal("city_consumption"));
                        details.HighwayConsumption = reader.GetString(reader.GetOrdinal("highway_consumption"));
                        details.MaxSpeed = reader.GetString(reader.GetOrdinal("max_speed"));
                        details.Acceleration = reader.GetString(reader.GetOrdinal("acceleration"));
                        details.CarBodyType = reader.IsDBNull(reader.GetOrdinal("body_type")) ? null : reader.GetString(reader.GetOrdinal("body_type"));
                    }
                }
            }
            Debug.WriteLine($"Car Details Loaded: CarId={details.CarId}, VIN={details.VIN}, Year={details.Year}, Color={details.Color}, HorsePower={details.HorsePower}, Price={details.Price}, ModelName={details.ModelName}, TrimName={details.TrimName}, Status={details.Status}");

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

        public void SellCar(int carId, int userId, int customerId, string customerFirstName, string customerLastName, string customerEmail, string customerPhone)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Проверка существования пользователя
                using (var userCheckCmd = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE user_id = @user_id", connection))
                {
                    userCheckCmd.Parameters.AddWithValue("user_id", userId);
                    var userExists = (long)userCheckCmd.ExecuteScalar() > 0;

                    if (!userExists)
                    {
                        // Создать нового пользователя, если он не существует
                        using (var createUserCmd = new NpgsqlCommand("INSERT INTO users (user_id, username, password_hash, salt) VALUES (@user_id, @username, @password_hash, @salt)", connection))
                        {
                            createUserCmd.Parameters.AddWithValue("user_id", userId);
                            createUserCmd.Parameters.AddWithValue("username", "new_user"); // или другое имя пользователя
                            createUserCmd.Parameters.AddWithValue("password_hash", "default_hash"); // установите по умолчанию
                            createUserCmd.Parameters.AddWithValue("salt", "default_salt"); // установите по умолчанию
                            createUserCmd.ExecuteNonQuery();
                        }
                    }
                }

                // Проверка существования клиента
                using (var customerCheckCmd = new NpgsqlCommand("SELECT COUNT(*) FROM customers WHERE customer_id = @customer_id", connection))
                {
                    customerCheckCmd.Parameters.AddWithValue("customer_id", customerId);
                    var customerExists = (long)customerCheckCmd.ExecuteScalar() > 0;

                    if (!customerExists)
                    {
                        // Создать нового клиента, если он не существует
                        using (var createCustomerCmd = new NpgsqlCommand("INSERT INTO customers (customer_id, first_name, last_name, email, phone) VALUES (@customer_id, @first_name, @last_name, @email, @phone)", connection))
                        {
                            createCustomerCmd.Parameters.AddWithValue("customer_id", customerId);
                            createCustomerCmd.Parameters.AddWithValue("first_name", customerFirstName);
                            createCustomerCmd.Parameters.AddWithValue("last_name", customerLastName);
                            createCustomerCmd.Parameters.AddWithValue("email", customerEmail);
                            createCustomerCmd.Parameters.AddWithValue("phone", customerPhone);
                            createCustomerCmd.ExecuteNonQuery();
                        }
                    }
                }

                // Вставка данных о продаже
                using (var cmd = new NpgsqlCommand("INSERT INTO sales (car_id, user_id, customer_id, sale_date) VALUES (@car_id, @user_id, @customer_id, @sale_date)", connection))
                {
                    cmd.Parameters.AddWithValue("car_id", carId);
                    cmd.Parameters.AddWithValue("user_id", userId);
                    cmd.Parameters.AddWithValue("customer_id", customerId);
                    cmd.Parameters.AddWithValue("sale_date", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }

                // Обновление статуса автомобиля
                using (var cmd = new NpgsqlCommand("UPDATE cars SET status = 'Sold' WHERE car_id = @car_id", connection))
                {
                    cmd.Parameters.AddWithValue("car_id", carId);
                    cmd.ExecuteNonQuery();
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Text.Json;

namespace AdsWithLogin.Data
{

    public class Repository
    {
        private string _connectionString;
        public Repository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void NewUser(User user, string password)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Users VALUES (@name, @email, @passwordHash)";
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            command.Parameters.AddWithValue("@name", user.Name);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@passwordHash", passwordHash);
            connection.Open();
            command.ExecuteNonQuery();
        }

        public void NewAd(Ad ad)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();

            if (ad.Name == null)
            {
                command.CommandText = "INSERT INTO Ads (DateCreated, Phone, Description)" +
                "VALUES (@dateCreated, @phone, @description)";
            }
            else
            {
                command.CommandText = "INSERT INTO Ads (Name, DateCreated, Phone, Description)" +
              "VALUES (@name, @dateCreated, @phone, @description)";
                command.Parameters.AddWithValue("@name", ad.Name);
            }
            command.Parameters.AddWithValue("@userId", ad.UserId);
            command.Parameters.AddWithValue("@dateCreated", DateTime.Now);
            command.Parameters.AddWithValue("@phone", ad.Phone);
            command.Parameters.AddWithValue("@description", ad.Description);

            connection.Open();

            command.ExecuteNonQuery();
        }
        public List<Ad> GetAllAds()
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Ads ORDER BY DateCreated DESC";
            connection.Open();
            List<Ad> ads = new List<Ad>();
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ads.Add(new Ad
                {
                    Id = (int)reader["Id"],
                    UserId = (int)reader["Id"],
                    DateCreated = (DateTime)reader["DateCreated"],
                    Phone = (string)reader["Phone"],
                    Description = (string)reader["Description"],
                });
            }
            return ads;
        }
        public List<Ad> GetAdsByUser(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Ads WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            connection.Open();
            var list = new List<Ad>();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Ad
                {
                    Id = (int)reader["Id"],
                    UserId = (int)reader["UserId"],
                    Phone = (string)reader["PhoneNumber"],
                    Description = (string)reader["Description"],
                    DateCreated = (DateTime)reader["Date"],
                });
            }
            return list;
        }
        public string GetUserName(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Name FROM Users WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            connection.Open();
            var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }
            return (string)reader["Name"];
        }
        public User Login(string email, string password)
        {
            var user = GetByEmail(email);
            if (user == null)
            {
                return null;
            }

            var isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!isValid)
            {
                return null;
            }

            return user;
        }
        private User GetByEmail(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Users WHERE Email = @email";
            cmd.Parameters.AddWithValue("@email", email);
            connection.Open();
            var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }
            return new User
            {
                Id = (int)reader["Id"],
                Name = (string)reader["Name"],
                Email = (string)reader["Email"],
                PasswordHash = (string)reader["PasswordHash"],
            };
        }
        public int GetUserIdByEmail(string email)
        {
            if (email == null)
            {
                return -1;
            }
            var user = GetByEmail(email);
            return user.Id;
        }
        public void DeleteAd(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Ads WHERE Id = @id ";
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            command.ExecuteNonQuery();
        }

    }

    public static class Extensions
    {
        public static T GetOrNull<T>(this SqlDataReader reader, string columnName)
        {
            var value = reader[columnName];
            if (value == DBNull.Value)
            {
                return default(T);
            }
            return (T)value;
        }
    }
}

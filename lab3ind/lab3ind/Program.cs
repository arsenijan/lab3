using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace lab3ind
{
    internal class Program
    {
        static string connectionString = "Server=localhost;Database=lab1;User ID=root;Password=Anikda13;";

        static void Main(string[] args)
        {
            int choice;
            do
            {
                Console.WriteLine("1. Знайти загальну суму, сплачену кожним абонентом на певну дату");
                Console.WriteLine("2. Отримати деталі платежів для кожного абонента");
                Console.WriteLine("3. Застосовати знижку 10% для певної категорії привілеїв");
                Console.WriteLine("4. Отримати загальну суму платежів для кожного абонента за поточний рік");
                Console.WriteLine("5. Знайти абонентів, які зовсім не сплачували за телефон");
                Console.WriteLine("0. Вийти");
                Console.Write("Введіть ваш вибір: ");
                choice = Convert.ToInt32(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        GetTotalAmountPaidByDate();
                        break;
                    case 2:
                        GetPaymentDetailsForSubscribers();
                        break;
                    case 3:
                        ApplyDiscountForPrivilegeCategory();
                        break;
                    case 4:
                        GetTotalPaymentsForCurrentYear();
                        break;
                    case 5:
                        FindSubscribersWithNoPayments();
                        break;
                    case 0:
                        Console.WriteLine("Exiting...");
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            } while (choice != 0);
        }

        static void GetTotalAmountPaidByDate()
        {
            Console.Write("Введіть дату (рік-місяць-день): ");
            string date = Console.ReadLine();
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT s.First_name, s.Last_name, s.Phone_number, SUM(p.Paid_amount_of_city_calls + p.Paid_amount_of_long_distance + p.Paid_amount_of_international) AS TotalAmount
                                 FROM subscriber s
                                 JOIN plateji p ON s.Phone_number = p.Phone_number
                                 WHERE DATE(p.Payment_date) = @date
                                 GROUP BY s.Phone_number";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@date", date);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"Name: {reader["First_name"]} {reader["Last_name"]}, Phone: {reader["Phone_number"]}, Total Amount: {reader["TotalAmount"]}");
                        }
                    }
                }
            }
        }

        static void GetPaymentDetailsForSubscribers()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT s.First_name, s.Last_name, p.Payment_date, p.Paid_amount_of_city_calls, p.Paid_amount_of_long_distance, p.Paid_amount_of_international,
                                        s.Non_payment_of_city_calls, s.Non_payment_of_long_distance, s.Non_payment_international
                                 FROM subscriber s
                                 JOIN plateji p ON s.Phone_number = p.Phone_number";
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"Name: {reader["First_name"]} {reader["Last_name"]}, Payment Date: {reader["Payment_date"]}");
                            Console.WriteLine($"Paid City Calls: {reader["Paid_amount_of_city_calls"]}, Paid Long Distance: {reader["Paid_amount_of_long_distance"]}, Paid International: {reader["Paid_amount_of_international"]}");
                            Console.WriteLine($"Non-Payment City Calls: {reader["Non_payment_of_city_calls"]}, Non-Payment Long Distance: {reader["Non_payment_of_long_distance"]}, Non-Payment International: {reader["Non_payment_international"]}");
                        }
                    }
                }
            }
        }

        static void ApplyDiscountForPrivilegeCategory()
        {
            Console.Write("Enter the privilege category: ");
            string privilege = Console.ReadLine();
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = @"UPDATE tariff SET Payment = Payment * 0.9 WHERE Privillage = @privilege";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@privilege", privilege);
                    int rowsAffected = command.ExecuteNonQuery();
                    Console.WriteLine($"{rowsAffected} rows updated.");
                }
            }
        }

        static void GetTotalPaymentsForCurrentYear()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT s.First_name, s.Last_name, s.Phone_number,
                                        SUM(p.Paid_amount_of_city_calls) AS TotalCityCalls,
                                        SUM(p.Paid_amount_of_long_distance) AS TotalLongDistance,
                                        SUM(p.Paid_amount_of_international) AS TotalInternational,
                                        SUM(p.Paid_amount_of_city_calls + p.Paid_amount_of_long_distance + p.Paid_amount_of_international) AS TotalAmount
                                 FROM subscriber s
                                 JOIN plateji p ON s.Phone_number = p.Phone_number
                                 WHERE YEAR(p.Payment_date) = YEAR(CURDATE())
                                 GROUP BY s.Phone_number";
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"Name: {reader["First_name"]} {reader["Last_name"]}, Phone: {reader["Phone_number"]}");
                            Console.WriteLine($"Total City Calls: {reader["TotalCityCalls"]}, Total Long Distance: {reader["TotalLongDistance"]}, Total International: {reader["TotalInternational"]}");
                            Console.WriteLine($"Total Amount: {reader["TotalAmount"]}");
                        }
                    }
                }
            }
        }

        static void FindSubscribersWithNoPayments()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT s.First_name, s.Last_name, s.Phone_number
                                 FROM subscriber s
                                 LEFT JOIN plateji p ON s.Phone_number = p.Phone_number
                                 WHERE p.Phone_number IS NULL";
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"Name: {reader["First_name"]} {reader["Last_name"]}, Phone: {reader["Phone_number"]}");
                        }
                    }
                }
            }
        }
    }
}

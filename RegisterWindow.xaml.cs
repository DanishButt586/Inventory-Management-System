using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Inventory_Management_System_SFML
{
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["InventorySystem"].ConnectionString);
        }

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        private void btnSignUp_Click(object sender, RoutedEventArgs e)
        {
            string username = txtNewUsername.Text;
            string password = txtNewPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;

            // Basic validation
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Username and password cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Save the username and hashed password to the database
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand("INSERT INTO Users (UserName, PasswordHash) VALUES (@UserName, @PasswordHash)", connection);
                    command.Parameters.AddWithValue("@UserName", username);
                    command.Parameters.AddWithValue("@PasswordHash", HashPassword(password)); // Hash the password before saving

                    command.ExecuteNonQuery();
                }

                MessageBox.Show("Registration successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close(); // Close the registration window after successful registration
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void chkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            // Show both passwords as plain text
            txtNewPassword.Visibility = Visibility.Collapsed;
            txtNewPasswordTextBox.Visibility = Visibility.Visible;
            txtNewPasswordTextBox.Text = txtNewPassword.Password;

            txtConfirmPassword.Visibility = Visibility.Collapsed;
            txtConfirmPasswordTextBox.Visibility = Visibility.Visible;
            txtConfirmPasswordTextBox.Text = txtConfirmPassword.Password;
        }

        private void chkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            // Hide the TextBoxes and show the PasswordBoxes again
            txtNewPasswordTextBox.Visibility = Visibility.Collapsed;
            txtNewPassword.Visibility = Visibility.Visible;

            txtConfirmPasswordTextBox.Visibility = Visibility.Collapsed;
            txtConfirmPassword.Visibility = Visibility.Visible;
        }

        private void txtNewPasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Update the PasswordBox when the TextBox changes
            txtNewPassword.Password = txtNewPasswordTextBox.Text;
        }

        private void txtConfirmPasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Update the PasswordBox when the TextBox changes
            txtConfirmPassword.Password = txtConfirmPasswordTextBox.Text;
        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

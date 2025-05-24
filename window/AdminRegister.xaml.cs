using ELMS_Group1.database;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace ELMS_Group1.window
{
    /// <summary>
    /// Interaction logic for AdminRegister.xaml
    /// </summary>
    public partial class AdminRegister : Window
    {
        private readonly SupabaseService supabaseService = new SupabaseService();

        public AdminRegister()
        {
            InitializeComponent();
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please fill out all the required fields.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match. Please try again.", "Password Mismatch", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var (success, message) = await supabaseService.RegisterAdminAsync(fullName, email, phone, password);

            if (success)
            {
                MessageBox.Show($"Account created successfully!\n\nWelcome, {fullName}!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                AdminLogin login = new AdminLogin();
                login.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show($"Account creation failed.\n\nDetails: {message}", "Registration Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            AdminLogin login = new AdminLogin();
            login.Show();
            this.Close();
        }
    }
}
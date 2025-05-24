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
    /// Interaction logic for UserRegister.xaml
    /// </summary>
    public partial class UserRegister : Window
    {
        SupabaseService supabaseService = new SupabaseService();
        public UserRegister()
        {
            InitializeComponent();
        }
        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string idNumber = IdnumberTextBox.Text.Trim();
            string address = AddressTextBox.Text.Trim();
            string courseYear = (CourseYearComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(idNumber) ||
                string.IsNullOrWhiteSpace(address) ||
                string.IsNullOrWhiteSpace(courseYear) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Please fill in all required fields.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Please enter a valid email address.", "Invalid Email", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var (success, message) = await supabaseService.SubmitPendingUserApplicationAsync(
                fullName,
                idNumber,
                courseYear,
                email,
                address,
                password
            );

            if (success)
            {
                MessageBox.Show("Registration submitted successfully! Please wait for admin approval.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close(); // Close registration window
            }
            else
            {
                MessageBox.Show($"Registration failed: {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {

            var login = new UserLogin();
            login.Show();
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

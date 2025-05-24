using ELMS_Group1.database;
using ELMS_Group1.model;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ELMS_Group1.window
{
    public partial class EditUserWindow : Window
    {
        private User _user;
        private SupabaseService supabaseService = new SupabaseService();
        public User UpdatedUser => _user;

        public EditUserWindow(User user)
        {
            InitializeComponent();

            _user = user ?? throw new ArgumentNullException(nameof(user));
            LoadUserData();
        }

        private void LoadUserData()
        {
            txtFullName.Text = _user.FullName;
            txtIdNumber.Text = _user.IdNumber;
            txtCourseYear.Text = _user.CourseYear;
            txtEmail.Text = _user.Email;
            txtAddress.Text = _user.Address;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate all fields
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Full Name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFullName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtIdNumber.Text))
            {
                MessageBox.Show("ID Number cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtIdNumber.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCourseYear.Text))
            {
                MessageBox.Show("Course/Year cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCourseYear.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Email cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return;
            }

            // Email format validation using regex
            if (!IsValidEmail(txtEmail.Text.Trim()))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Address cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAddress.Focus();
                return;
            }

            // Update user object
            _user.FullName = txtFullName.Text.Trim();
            _user.IdNumber = txtIdNumber.Text.Trim();
            _user.CourseYear = txtCourseYear.Text.Trim();
            _user.Email = txtEmail.Text.Trim();
            _user.Address = txtAddress.Text.Trim();
            _user.UpdatedAt = DateTime.UtcNow;

            try
            {
                var (success, message) = await supabaseService.UpdateUserData(_user);
                if (!success)
                {
                    MessageBox.Show($"Failed to update user data: {message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while updating user data:\n{ex.Message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Simple email validation helper method
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            try
            {
                // Use regex to validate email
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return emailRegex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }
    }
}

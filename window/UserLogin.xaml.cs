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
    /// Interaction logic for UserLogin.xaml
    /// </summary>
    public partial class UserLogin : Window
    {
        SupabaseService supabaseService = new SupabaseService();
        public UserLogin()
        {
            InitializeComponent();
        }
        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string emailOrName = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(emailOrName) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter your email/full name and password.", "Missing Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1. Try User login first
            var userLogin = await supabaseService.UserLoginEmailorNameAsync(emailOrName, password);
            if (userLogin.success)
            {
                if (userLogin.user != null)
                {
                    // Successful login as approved user
                    MessageBox.Show($"Welcome, {userLogin.user.FullName}!", "Login Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Proceed to main app screen or dashboard
                    return;
                }
            }
            else if (userLogin.message != "User not found.")
            {
                // If failure was not "User not found" (e.g., invalid password), show error and stop
                MessageBox.Show(userLogin.message, "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 2. No user found, try PendingUser login
            var pendingLogin = await supabaseService.PendingUserLoginEmailorNameAsync(emailOrName, password);
            if (pendingLogin.success)
            {
                if (pendingLogin.pendingUser != null)
                {
                    var pUser = pendingLogin.pendingUser;
                    // Check approval status
                    if (pUser.IsApproved == null)
                    {
                        MessageBox.Show("Your application is still waiting for admin approval. Please check back later.", "Pending Approval", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    else if (pUser.IsApproved == false)
                    {
                        // Denied - show admin feedback and option to delete application
                        var res = MessageBox.Show($"Your application was denied.\nReason: {pUser.AdminFeedback}\n\nDo you want to delete your application so you can try to register again?",
                            "Application Denied", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (res == MessageBoxResult.Yes)
                        {
                            var deleteResult = await supabaseService.DeletePendingUserAsync(pUser.Id);
                            if (deleteResult.success)
                                MessageBox.Show("Application deleted. You can now register a new account.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                            else
                                MessageBox.Show($"Failed to delete application: {deleteResult.message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        return;
                    }
                    else if (pUser.IsApproved == true)
                    {
                        // Approved but might be deleted, option to delete application for remake
                        var res = MessageBox.Show("Your account is approved but might be deleted. Do you want to delete your application to remake your account?",
                            "Account Notice", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (res == MessageBoxResult.Yes)
                        {
                            var deleteResult = await supabaseService.DeletePendingUserAsync(pUser.Id);
                            if (deleteResult.success)
                                MessageBox.Show("Application deleted. You can now register a new account.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                            else
                                MessageBox.Show($"Failed to delete application: {deleteResult.message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        return;
                    }
                }
            }
            else if (pendingLogin.message != "Pending user not found.")
            {
                MessageBox.Show(pendingLogin.message, "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 3. If no user or pending user found
            MessageBox.Show("No account found with that email or full name.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }


        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var userRegister = new UserRegister();
            userRegister.ShowDialog();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
  }


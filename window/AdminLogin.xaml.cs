using ELMS_Group1.database;
using ELMS_Group1.window;
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

namespace ELMS_Group1
{
    /// <summary>
    /// Interaction logic for AdminLogin.xaml
    /// </summary>
    public partial class AdminLogin : Window
    {
        private readonly SupabaseService supabaseService = new SupabaseService();
        public AdminLogin()
        {
            InitializeComponent();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextbox.Text.Trim();
            string password = PasswordBox.Password;

            var (success, message, admin) = await supabaseService.VerifyAdminLoginAsync(email, password);

            if (success)
            {
                MessageBox.Show("Login successful!");
                AdminDashboard dashboard = new AdminDashboard(admin);
                dashboard.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid email or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Register_Click(object sender, RoutedEventArgs e)
        {
           AdminRegister register = new AdminRegister();
            register.Show();
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}

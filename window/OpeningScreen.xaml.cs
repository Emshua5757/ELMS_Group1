using ELMS_Group1.database;
using System.Windows;
using System.Windows.Input;

namespace ELMS_Group1.window
{
    public partial class OpeningScreen : Window
    {
        SupabaseService _supabaseService;

        public OpeningScreen()
        {
            InitializeComponent();
            _supabaseService = new SupabaseService();

            this.Loaded += (s, e) => Keyboard.Focus(this);
        }

        private void UserLoginButton_Click(object sender, RoutedEventArgs e)
        {
            UserLogin userLoginWindow = new UserLogin();
            userLoginWindow.Show();
            this.Close();
        }

        private void UserRegisterButton_Click(object sender, RoutedEventArgs e)
        {
            UserRegister userRegisterWindow = new UserRegister();
            userRegisterWindow.Show();
            this.Close();
        }

        private void AdminLogin_Click(object sender, RoutedEventArgs e)
        {
            AdminLogin adminLoginWindow = new AdminLogin();
            adminLoginWindow.Show();
            this.Close();
        }

        private void AdminDashboard_Click(object sender, RoutedEventArgs e)
        {
            AdminDashboard adminDashboardWindow = new AdminDashboard(null);
            adminDashboardWindow.Show();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshButton.Visibility = Visibility.Collapsed;
            AdminLoginButton.Visibility = Visibility.Collapsed;
            AdminDashboardButton.Visibility = Visibility.Collapsed;
        }



        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.A)
            {
                AdminLoginButton.Visibility = Visibility.Visible;
                AdminDashboardButton.Visibility = Visibility.Visible;
                RefreshButton.Visibility = Visibility.Visible; 
                MessageBox.Show("Admin options unlocked!");
            }
        }

        private void LeftBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }


    }
}

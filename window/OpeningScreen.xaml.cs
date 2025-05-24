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
    /// Interaction logic for OpeningScreen.xaml
    /// </summary>
    public partial class OpeningScreen : Window
    {
        SupabaseService _supabaseService;
        public OpeningScreen()
        {
            InitializeComponent();
            _supabaseService = new SupabaseService();

            // Load data asynchronously after window loads
            this.Loaded += OpeningScreen_Loaded;
        }

        private void AdminLogin_Click(object sender, RoutedEventArgs e)
        {
            AdminLogin adminLoginWindow = new AdminLogin();
            adminLoginWindow.Show(); 
        }
        private void AdminDashboard_Click(object sender, RoutedEventArgs e)
        {
            AdminDashboard adminDashboardWindow = new AdminDashboard(null);
            adminDashboardWindow.Show();
        }
        private async void OpeningScreen_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAdminsAsync();
            await LoadUsersAsync();
            await LoadInventoryAsync();
            await LoadBorrowStatusAsync();
        }

        private async Task LoadAdminsAsync()
        {
            var result = await _supabaseService.SearchAdminsAsync("");
            if (result.success && result.admins != null)
            {
                AdminsDataGrid.ItemsSource = result.admins;
            }
            else
            {
                MessageBox.Show($"Failed to load admins: {result.message}");
            }
        }

        private async Task LoadUsersAsync()
        {
            var result = await _supabaseService.SearchUsersAsync(""); // Empty search = all users
            if (result.success && result.users != null)
            {
                UsersDataGrid.ItemsSource = result.users;
            }
            else
            {
                MessageBox.Show($"Failed to load users: {result.message}");
            }
        }

        private async Task LoadInventoryAsync()
        {
            var result = await _supabaseService.SearchInventoryItemsAsync("");
            if (result.success && result.items != null)
            {
                InventoryDataGrid.ItemsSource = result.items;
            }
            else
            {
                MessageBox.Show($"Failed to load inventory: {result.message}");
            }
        }

        private async Task LoadBorrowStatusAsync()
        {
            var result = await _supabaseService.SearchBorrowedStatusAsync("");
            if (result.success && result.records != null)
            {
                BorrowStatusDataGrid.ItemsSource = result.records;
            }
            else
            {
                MessageBox.Show($"Failed to load borrow status: {result.message}");
            }
        }
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshAllDataAsync();
        }

        private async Task RefreshAllDataAsync()
        {
            await LoadAdminsAsync();
            await LoadUsersAsync();
            await LoadInventoryAsync();
            await LoadBorrowStatusAsync();
        }

        private void UserLoginButton_Click(object sender, RoutedEventArgs e)
        {
            UserLogin userLoginWindow = new UserLogin();
            userLoginWindow.Show();
        }
    }
}

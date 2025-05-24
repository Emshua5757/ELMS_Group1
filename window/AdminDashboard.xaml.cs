using ELMS_Group1.database;
using ELMS_Group1.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ELMS_Group1.window
{
    /// <summary>
    /// Interaction logic for AdminDashboard.xaml
    /// </summary>
    public partial class AdminDashboard : Window
    {
        Admin? admin;
        SupabaseService supabaseService = new SupabaseService();
        public AdminDashboard(Admin admin)
        {
            this.admin = admin;
            InitializeComponent();
            ShowSection(InventoryBtn, new RoutedEventArgs());
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private async void ShowSection(object sender, RoutedEventArgs e)
        {

            InventorySection.Visibility = Visibility.Collapsed;
            UsersSection.Visibility = Visibility.Collapsed;
            PendingSection.Visibility = Visibility.Collapsed;
            BorrowedSection.Visibility = Visibility.Collapsed;
            ReportsSection.Visibility = Visibility.Collapsed;
            AdminSection.Visibility = Visibility.Collapsed;
            ApplicationsSection.Visibility = Visibility.Collapsed;

            InventoryBtn.Background = Brushes.LightGray;
            UsersBtn.Background = Brushes.LightGray;
            ApplicationsBtn.Background = Brushes.LightGray;
            PendingBtn.Background = Brushes.LightGray;
            BorrowedBtn.Background = Brushes.LightGray;
            ReportsBtn.Background = Brushes.LightGray;
            AdminBtn.Background = Brushes.LightGray;


            Button clickedButton = (Button)sender;
            clickedButton.Background = Brushes.LightBlue;

            if (clickedButton == InventoryBtn)
                InventorySection.Visibility = Visibility.Visible;
            else if (clickedButton == UsersBtn)
            {
                await LoadUsers(UsersGrid);
                UsersSection.Visibility = Visibility.Visible;
            }
            else if (clickedButton == ApplicationsBtn)
            {
                await LoadPendingApplications(ApplicationsGrid);
                ApplicationsSection.Visibility = Visibility.Visible;
            }
            else if (clickedButton == PendingBtn)
                PendingSection.Visibility = Visibility.Visible;
            else if (clickedButton == BorrowedBtn)
                BorrowedSection.Visibility = Visibility.Visible;
            else if (clickedButton == ReportsBtn)
                ReportsSection.Visibility = Visibility.Visible;
            else if (clickedButton == AdminBtn)
                AdminSection.Visibility = Visibility.Visible;
        }
        private void AddInventory_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Add Inventory feature not implemented yet.", "Add Inventory", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void EditInventory_Click(object sender, RoutedEventArgs e)
        {
            if (InventoryGrid.SelectedItem == null)
            {
                MessageBox.Show("Please select an inventory item to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Edit Inventory feature is under development.", "Edit Inventory", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void RemoveInventory_Click(object sender, RoutedEventArgs e)
        {
            if (InventoryGrid.SelectedItem == null)
            {
                MessageBox.Show("Please select an inventory item to remove.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Remove Inventory feature is under development.", "Remove Inventory", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void AcceptRequestBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedRequest = PendingGrid.SelectedItem;
            if (selectedRequest == null)
            {
                MessageBox.Show("Please select a borrow request first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PendingDatePicker.SelectedDate == null || DueDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please select both Accepted Date and Due Date.", "Missing Dates", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Borrow request accepted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void RejectRequestBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedRequest = PendingGrid.SelectedItem;
            if (selectedRequest == null)
            {
                MessageBox.Show("Please select a borrow request first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            MessageBox.Show("Borrow request rejected.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private async void ApproveApplication_Click(object sender, RoutedEventArgs e)
        {
            var selectedApplication = ApplicationsGrid.SelectedItem as PendingUser;
            if (selectedApplication == null)
            {
                MessageBox.Show("Please select an application to approve.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (admin == null)
            {
                MessageBox.Show("Admin information is missing. Please re-login.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            (bool success, string message) = await supabaseService.ApproveAndAddUserAsync(selectedApplication.Id, admin.FullName);

            if (success)
            {
                MessageBox.Show("Application approved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadPendingApplications(ApplicationsGrid);
            }
            else
            {
                MessageBox.Show($"Failed to approve application: {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void RejectApplication_Click(object sender, RoutedEventArgs e)
        {
            var selectedApplication = ApplicationsGrid.SelectedItem as PendingUser;
            if (selectedApplication == null)
            {
                MessageBox.Show("Please select an application to reject.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var inputDialog = new InputDialog("Enter rejection message:", "Please provide a reason for rejection:");
            if (inputDialog.ShowDialog() != true || string.IsNullOrWhiteSpace(inputDialog.ResponseText))
            {
                MessageBox.Show("Rejection message is required.", "Missing Message", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (admin == null)
            {
                MessageBox.Show("Admin information is missing. Please re-login.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            (bool success, string message) = await supabaseService.RejectPendingUserAsync(selectedApplication.Id, inputDialog.ResponseText, admin.Email);

            if (success)
            {
                MessageBox.Show("Application rejected successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadPendingApplications(ApplicationsGrid);
            }
            else
            {
                MessageBox.Show($"Failed to reject application: {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void RegisterUser_Click(object sender, RoutedEventArgs e)
        {
            UserRegister userRegister = new UserRegister();
            userRegister.Show();
        }
        private async Task LoadPendingApplications(DataGrid applicationsGrid, string? search = "")
        {
            var (success, users, message) = await supabaseService.SearchPendingUsersAsync(search);

            if (success && users != null)
            {
                applicationsGrid.ItemsSource = users;
            }
            else
            {
                MessageBox.Show($"Failed to load pending users: {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ApplicationsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = ApplicationsGrid.SelectedItem as PendingUser;

            if (selected == null)
            {
                ApproveButton.IsEnabled = false;
                RejectButton.IsEnabled = false;
                return;
            }

            if (selected.IsApproved != null)
            {
                ApproveButton.IsEnabled = false;
                RejectButton.IsEnabled = false;
            }
            else
            {
                ApproveButton.IsEnabled = true;
                RejectButton.IsEnabled = true;
            }
        }
        private async void ApplicationsSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ApplicationsSearchBox == null)
                return;
            if (ApplicationsGrid == null)
                return;
            if (ApplicationsSearchBox.Text == "Search by name or ID...")
            {
                await LoadPendingApplications(ApplicationsGrid);
                return;
            }

            string searchText = ApplicationsSearchBox.Text.Trim();
            await LoadPendingApplications(ApplicationsGrid, searchText);
        }
        private void ApplicationsSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ApplicationsSearchBox.Text == "Search by name or ID...")
            {
                ApplicationsSearchBox.Text = "";
                ApplicationsSearchBox.Foreground = Brushes.Black;
            }
        }
        private void ApplicationsSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ApplicationsSearchBox.Text))
            {
                ApplicationsSearchBox.Text = "Search by name or ID...";
                ApplicationsSearchBox.Foreground = Brushes.Gray;
            }
        }
        private async Task LoadUsers(DataGrid usersGrid, string? search = "")
        {
            var (success, users, message) = await supabaseService.SearchUsersAsync(search);

            if (success && users != null)
            {
                usersGrid.ItemsSource = users;
            }
            else
            {
                MessageBox.Show($"Failed to load users: {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void UsersSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UsersSearchBox.Text == "Search by name or ID...")
            {
                UsersSearchBox.Text = "";
                UsersSearchBox.Foreground = Brushes.Black;
            }
        }
        private void UsersSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsersSearchBox.Text))
            {
                UsersSearchBox.Text = "Search by name or ID...";
                UsersSearchBox.Foreground = Brushes.Gray;
            }
        }
        private async void UsersSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UsersSearchBox == null)
                return;
            if (UsersGrid == null)
                return;
            if (UsersSearchBox.Text == "Search by name or ID...")
            {
                await LoadUsers(UsersGrid);
                return;
            }
            string searchText = UsersSearchBox.Text.Trim();
            await LoadUsers(UsersGrid, searchText);
        }
        private void UsersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = UsersGrid.SelectedItem as User;

            if (selected == null)
            {
                EditUserButton.IsEnabled = false;
                RemoveUserButton.IsEnabled = false;
                return;
            }
            else
            {
                EditUserButton.IsEnabled = true;
                RemoveUserButton.IsEnabled = true;
            }
        }
        private async void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is User selectedUser)
            {
                var editWindow = new EditUserWindow(selectedUser);

                bool? dialogResult = editWindow.ShowDialog();

                if (dialogResult == true)
                {
                    User updatedUser = editWindow.UpdatedUser;

                    await LoadUsers(UsersGrid);
                }
            }
            else
            {
                MessageBox.Show("Please select a user to edit.", "No User Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void RemoveUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is User selectedUser)
            {
                var confirm = MessageBox.Show($"Are you sure you want to delete user '{selectedUser.FullName}'?",
                                              "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm == MessageBoxResult.Yes)
                {
                    var result = await supabaseService.DeleteUserAsync(selectedUser.Id); // your Supabase delete method

                    if (result.success)
                    {
                        MessageBox.Show("User deleted successfully.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadUsers(UsersGrid);  // Refresh user list/grid
                    }
                    else
                    {
                        MessageBox.Show($"Delete failed: {result.message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

    }
}

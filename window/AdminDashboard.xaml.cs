using ELMS_Group1.database;
using ELMS_Group1.model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private List<BorrowedStatus> _allProblematicBorrows = new List<BorrowedStatus>();
        Admin? admin;
        SupabaseService supabaseService = new SupabaseService();
        private bool sortReportsNewestFirst = true;
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
            // Hide all sections
            InventorySection.Visibility = Visibility.Collapsed;
            UsersSection.Visibility = Visibility.Collapsed;
            PendingSection.Visibility = Visibility.Collapsed;
            BorrowedSection.Visibility = Visibility.Collapsed;
            ReportsSection.Visibility = Visibility.Collapsed;
            AdminSection.Visibility = Visibility.Collapsed;
            ApplicationsSection.Visibility = Visibility.Collapsed;
            ProblematicSection.Visibility = Visibility.Collapsed;
            BorrowHistorySection.Visibility = Visibility.Collapsed;

            // Reset all button backgrounds
            InventoryBtn.Background = Brushes.LightGray;
            UsersBtn.Background = Brushes.LightGray;
            ApplicationsBtn.Background = Brushes.LightGray;
            PendingBtn.Background = Brushes.LightGray;
            BorrowedBtn.Background = Brushes.LightGray;
            ReportsBtn.Background = Brushes.LightGray;
            AdminBtn.Background = Brushes.LightGray;
            ProblematicBtn.Background = Brushes.LightGray;
            HistoryBtn.Background = Brushes.LightGray;

            // Highlight the clicked button
            Button clickedButton = (Button)sender;
            clickedButton.Background = Brushes.LightBlue;

            // Show the corresponding section
            if (clickedButton == InventoryBtn)
            {
                await LoadInventory(InventoryGrid);
                InventorySection.Visibility = Visibility.Visible;
            }
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
            {
                await LoadPendingBorrowRequests(PendingGrid);
                PendingSection.Visibility = Visibility.Visible;
            }
            else if (clickedButton == BorrowedBtn)
            {
                await LoadBorrowedRequests(BorrowedGrid);
                BorrowedSection.Visibility = Visibility.Visible;
            }
            else if (clickedButton == ProblematicBtn)
            {
                await LoadProblematicHistoryRequests(); 
                ProblematicSection.Visibility = Visibility.Visible;
            }
            else if (clickedButton == HistoryBtn)
            {
                await LoadBorrowHistoryRequests(BorrowHistoryGrid);
                await GetBorrowHistoryStats();
                BorrowHistorySection.Visibility = Visibility.Visible;
            }
            else if (clickedButton == ReportsBtn)
            {
                await LoadReportsAsync();
                LoadStatistics();
                ReportsSection.Visibility = Visibility.Visible;
            }
            else if (clickedButton == AdminBtn)
            {
                await LoadAdminsAsync();
                AdminSection.Visibility = Visibility.Visible;
            }
        }
        private async Task LoadPendingBorrowRequests(DataGrid pendingGrid, string? search = "")
        {
            var (success, records, message) = await supabaseService.SearchBorrowedStatusAsync(search ?? "", "Pending");
            if (pendingGrid == null) return;
            if (success && records != null)
            {
                pendingGrid.ItemsSource = records;
            }
            else
            {
                MessageBox.Show($"Failed to load pending borrow requests: {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void PendingGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // var selected = PendingGrid.SelectedItem as BorrowStatus;
        }
        private async void PendingSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PendingSearchBox == null)
                return;
            if (PendingSearchBox.Text == "Search by name or ID...")
            {
                await LoadPendingBorrowRequests(PendingGrid);
                return;
            }

            string searchText = PendingSearchBox.Text.Trim();
            await LoadPendingBorrowRequests(PendingGrid, searchText);
        }
        private void PendingSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PendingSearchBox.Text == "Search by name or ID...")
            {
                PendingSearchBox.Text = "";
                PendingSearchBox.Foreground = Brushes.Black;
            }
        }
        private void PendingSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PendingSearchBox.Text))
            {
                PendingSearchBox.Text = "Search by name or ID...";
                PendingSearchBox.Foreground = Brushes.Gray;
            }
        }
        private async void AddInventory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddInventoryOptionDialog();
            if (dialog.ShowDialog() == true)
            {
                switch (dialog.SelectedOption)
                {
                    case AddInventoryOptionDialog.InventoryAddOption.Single:
                        var singleWindow = new AddSingleInventoryWindow();
                        bool? singleResult = singleWindow.ShowDialog();
                        if (singleResult == true)
                        {
                            var item = singleWindow.InventoryItem;
                            long? adminId = admin?.Id;

                            var (success, message) = await supabaseService.AddSingleInventoryItemAsync(item, adminId);
                            MessageBox.Show(message, success ? "Success" : "Error",
                                MessageBoxButton.OK,
                                success ? MessageBoxImage.Information : MessageBoxImage.Error);
                        }
                        break;

                    case AddInventoryOptionDialog.InventoryAddOption.Multiple:
                        {
                            var multipleWindow = new AddMultipleInventoryWindow();
                            bool? multipleResult = multipleWindow.ShowDialog();
                            if (multipleResult == true)
                            {
                                var items = multipleWindow.InventoryItems;
                                long? adminId = admin?.Id;

                                var (success, message) = await supabaseService.AddMultipleInventoryItemsAsync(items, adminId);

                                MessageBox.Show(message,
                                    success ? "Success" : "Error",
                                    MessageBoxButton.OK,
                                    success ? MessageBoxImage.Information : MessageBoxImage.Error);
                            }
                            break;
                        }
                    case AddInventoryOptionDialog.InventoryAddOption.Csv:
                        var inputDialog = new InputDialog(
                            "Add Inventory from CSV",
                            "Paste your CSV content below:"
                        );

                        bool? result = inputDialog.ShowDialog();
                        if (result == true)
                        {
                            string csvContent = inputDialog.ResponseText;
                            long? adminId = admin?.Id;

                            var (success, message) = await supabaseService.AddFromCsvAsync(csvContent, adminId);

                            MessageBox.Show(
                                message,
                                "CSV Import Result",
                                MessageBoxButton.OK,
                                success ? MessageBoxImage.Information : MessageBoxImage.Error
                            );
                        }
                        break;
                }
            }
            await LoadInventory(InventoryGrid);
        }
        private async void EditInventory_Click(object sender, RoutedEventArgs e)
        {
            if (InventoryGrid.SelectedItem == null)
            {
                MessageBox.Show("Please select an inventory item to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (InventoryGrid.SelectedItem is InventoryItem selectedItem)
            {
                var editWindow = new EditInventoryWindow(selectedItem);

                bool? result = editWindow.ShowDialog();

                if (result == true)
                {
                    InventoryItem updatedItem = editWindow.UpdatedInventoryItem;

                    var (success, message) = await supabaseService.UpdateInventoryItemData(updatedItem, admin);

                    MessageBox.Show(message, success ? "Updated" : "Error",
                        MessageBoxButton.OK,
                        success ? MessageBoxImage.Information : MessageBoxImage.Error);

                    if (success)
                    {
                        await LoadInventory(InventoryGrid);
                    }
                }
            }
        }
        private async void RemoveInventory_Click(object sender, RoutedEventArgs e)
        {
            if (InventoryGrid.SelectedItem == null)
            {
                MessageBox.Show("Please select an inventory item to remove.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (InventoryGrid.SelectedItem is InventoryItem selectedItem)
            {
                var confirm = MessageBox.Show(
                    $"Are you sure you want to delete '{selectedItem.Name}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm == MessageBoxResult.Yes)
                {
                    (bool success, string message) = await supabaseService.DeleteInventoryItemAsync(selectedItem.Id, admin);

                    MessageBox.Show(message, success ? "Deleted" : "Error",
                        MessageBoxButton.OK,
                        success ? MessageBoxImage.Information : MessageBoxImage.Error);

                    if (success)
                    {
                        await LoadInventory(InventoryGrid);
                    }
                }
            }
        }
        private void InventoryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InventoryGrid.SelectedItem is InventoryItem selectedItem)
            {
                EditInventoryButton.IsEnabled = true;
                RemoveInventoryButton.IsEnabled = true;
            }
            else
            {
                EditInventoryButton.IsEnabled = false;
                RemoveInventoryButton.IsEnabled = false;
            }
        }
        private async void InventorySearchBox_TextChanged(Object sender, TextChangedEventArgs e)
        {
            if (InventorySearchBox == null)
                return;
            if (InventorySearchBox == null)
                return;
            if (InventorySearchBox.Text == "Search...")
            {
                await LoadInventory(InventoryGrid);
                return;
            }

            string searchText = InventorySearchBox.Text.Trim();
            await LoadInventory(InventoryGrid, searchText);
        }
        private async Task LoadInventory(DataGrid inventoryGrid, string? search = "")
        {
            (bool success, List<InventoryItem>? items, string message) = await supabaseService.SearchInventoryItemsAsync(search);
            if (inventoryGrid == null)
            {
                return;
            }
            if (success && items != null)
            {
                inventoryGrid.ItemsSource = items;
            }
            else
            {
                MessageBox.Show($"Failed to load inventory items: {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void InventorySearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (InventorySearchBox.Text == "Search...")
            {
                InventorySearchBox.Text = "";
                InventorySearchBox.Foreground = Brushes.Black;
            }
        }
        private void InventorySearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InventorySearchBox.Text))
            {
                InventorySearchBox.Text = "Search...";
                InventorySearchBox.Foreground = Brushes.Gray;
            }
        }
        private async void AcceptRequestBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedRequest = PendingGrid.SelectedItem as BorrowedStatus;
            if (selectedRequest == null)
            {
                MessageBox.Show("Please select a borrow request first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DueDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please select a due date.", "Missing Due Date", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime dueDate = DueDatePicker.SelectedDate.Value;

            var (success, message) = await supabaseService.ApproveBorrowRequestAsync(selectedRequest.Id, admin, dueDate);

            if (success)
            {
                MessageBox.Show("Borrow request accepted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadPendingBorrowRequests(PendingGrid);  // refresh the grid after approval
            }
            else
            {
                MessageBox.Show($"Failed to accept borrow request: {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void RejectRequestBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedRequest = PendingGrid.SelectedItem as BorrowedStatus;
            if (selectedRequest == null)
            {
                MessageBox.Show("Please select a borrow request first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var (success, message) = await supabaseService.RejectBorrowRequestAsync(selectedRequest.Id, admin);

            if (success)
            {
                MessageBox.Show("Borrow request rejected.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadPendingBorrowRequests(PendingGrid);  // refresh the grid after rejection
            }
            else
            {
                MessageBox.Show($"Failed to reject borrow request: {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

            (bool success, string message) = await supabaseService.ApproveAndAddUserAsync(selectedApplication.Id, admin);

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

            (bool success, string message) = await supabaseService.RejectPendingUserAsync(selectedApplication.Id, inputDialog.ResponseText, admin);

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
                var editWindow = new EditUserWindow(selectedUser, admin);

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
                    var result = await supabaseService.DeleteUserAsync(selectedUser.Id, admin);

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
        private void ReportSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ReportSearchBox.Text == "Search by title or type...")
            {
                ReportSearchBox.Text = "";
                ReportSearchBox.Foreground = Brushes.Black;
            }
        }
        private void ReportSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ReportSearchBox.Text))
            {
                ReportSearchBox.Text = "Search by title or type...";
                ReportSearchBox.Foreground = Brushes.Gray;
            }
        }
        private async void ReportSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ReportSearchBox == null || ReportList == null)
                return;

            if (ReportSearchBox.Text == "Search by title or type...")
            {
                await LoadReportsAsync();
                return;
            }

            string query = ReportSearchBox.Text.Trim();
            await LoadReportsAsync(query);
        }
        private async Task LoadReportsAsync(string? searchText = "")
        {
            var (success, reports, message) = await supabaseService.SearchReportsAsync(searchText);

            if (!success || reports == null)
            {
                MessageBox.Show($"Failed to load reports: {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var sortedReports = sortReportsNewestFirst
                ? reports.OrderByDescending(r => r.CreatedAt)
                : reports.OrderBy(r => r.CreatedAt);

            ReportList.ItemsSource = sortedReports.Select(report => new ReportCard(report)).ToList();
        }
        private async void SortReportsButton_Click(object sender, RoutedEventArgs e)
        {
            sortReportsNewestFirst = !sortReportsNewestFirst;

            SortReportsText.Text = sortReportsNewestFirst ? "Sort: Newest" : "Sort: Oldest";

            await LoadReportsAsync(ReportSearchBox.Text == "Search by title or type..." ? "" : ReportSearchBox.Text);
        }
        private async void LoadStatistics()
        {
            var (success, stats, message) = await supabaseService.GetStats();

            if (success && stats != null)
            {
                AdminsCountText.Text = stats.Admins.ToString();
                UsersCountText.Text = stats.Users.ToString();
                PendingUsersCountText.Text = stats.PendingUsers.ToString();
                InventoryCountText.Text = stats.InventoryItems.ToString();
                BorrowedItemsCountText.Text = stats.BorrowedItems.ToString();
                PendingBorrowsCountText.Text = stats.PendingBorrows.ToString();
                OverdueItemsCountText.Text = stats.OverdueItems.ToString();
                DamagedItemsCountText.Text = stats.DamagedItems.ToString();
                LostItemsCountText.Text = stats.LostItems.ToString();
            }
            else
            {
                AdminsCountText.Text = "Error";
                UsersCountText.Text = "Error";
                PendingUsersCountText.Text = "Error";
                InventoryCountText.Text = "Error";
                BorrowedItemsCountText.Text = "Error";
                PendingBorrowsCountText.Text = "Error";
                OverdueItemsCountText.Text = "Error";
                DamagedItemsCountText.Text = "Error";
                LostItemsCountText.Text = "Error";

                MessageBox.Show($"Failed to load statistics: {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void AdminSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (AdminSearchBox.Text == "Search admins...")
            {
                AdminSearchBox.Text = "";
                AdminSearchBox.Foreground = Brushes.Black;
            }
        }
        private void AdminSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AdminSearchBox.Text))
            {
                AdminSearchBox.Text = "Search admins...";
                AdminSearchBox.Foreground = Brushes.Gray;
            }
        }
        private async void AdminSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AdminSearchBox == null || AdminCardsList == null)
                return;

            if (AdminSearchBox.Text == "Search admins...")
            {
                await LoadAdminsAsync();
                return;
            }

            string query = AdminSearchBox.Text.Trim();
            await LoadAdminsAsync(query);
        }
        private async Task LoadAdminsAsync(string? searchText = "")
        {
            var (success, admins, message) = await supabaseService.SearchAdminsAsync(searchText);

            if (!success || admins == null)
            {
                MessageBox.Show($"Failed to load admins: {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Populate AdminCardsList with simple UI cards
            AdminCardsList.ItemsSource = admins.Select(admin => new AdminCard(admin)).ToList();
        }
        private async void CheckOverdueBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckOverdueBtn.IsEnabled = false;

            var (success, message) = await supabaseService.CheckOverdueAsync();

            if (success)
            {
                MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ShowSection(ProblematicBtn, new RoutedEventArgs());
            }
            else
            {
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            CheckOverdueBtn.IsEnabled = true;
        }
        private async void ReturnBtn_Click(object sender, RoutedEventArgs e)
        {
            if (BorrowedGrid.SelectedItem is BorrowedStatus selectedStatus)
            {
                var dialog = new ReturnStatusDialog(selectedStatus);
                var result = dialog.ShowDialog();

                if (result == true)
                {
                    
                    string newStatus = dialog.SelectedStatus;

                    selectedStatus.Status = newStatus;
                    selectedStatus.UpdatedAt = DateTime.UtcNow;

                    if (newStatus == BorrowStatus.Returned.ToString())
                    {
                        selectedStatus.ReturnedDate = DateTime.UtcNow;
                    }

                    var (success, message) = await supabaseService.UpdateBorrowHistoryAsync(selectedStatus, admin);

                    if (success)
                    {
                        MessageBox.Show("Item status and inventory updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        ShowSection(HistoryBtn, new RoutedEventArgs());
                    }
                    else
                    {
                        MessageBox.Show($"Failed to update item status and inventory: {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select an item to return.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void BorrowedSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (BorrowedSearchBox.Text == "Search by name...")
            {
                BorrowedSearchBox.Text = "";
                BorrowedSearchBox.Foreground = Brushes.Black;
            }
        }
        private void BorrowedSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BorrowedSearchBox.Text))
            {
                BorrowedSearchBox.Text = "Search by name...";
                BorrowedSearchBox.Foreground = Brushes.Gray;
            }
        }
        private async void BorrowedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (BorrowedSearchBox == null || BorrowedGrid == null)
                return;

            if (BorrowedSearchBox.Text == "Search by name...")
            {
                await LoadBorrowedRequests(BorrowedGrid);
                return;
            }

            string query = BorrowedSearchBox.Text.Trim();
            await LoadBorrowedRequests(BorrowedGrid, query);
        }
        private void BorrowedGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        private async Task LoadBorrowedRequests(DataGrid borrowedGrid, string? search = "")
        {
            var (success, records, message) = await supabaseService.SearchBorrowedStatusAsync(search ?? "", "Borrowed");
            if (borrowedGrid == null) return;
            if (success && records != null)
            {
                borrowedGrid.ItemsSource = records;
            }
            else
            {
                MessageBox.Show($"Failed to load pending borrow requests: {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BorrowHistorySearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (BorrowHistorySearchBox.Text == "Search by name or status...")
            {
                BorrowHistorySearchBox.Text = "";
                BorrowHistorySearchBox.Foreground = Brushes.Black;
            }
        }
        private void BorrowHistorySearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BorrowedSearchBox.Text))
            {
                BorrowHistorySearchBox.Text = "Search by name or status...";
                BorrowHistorySearchBox.Foreground = Brushes.Gray;
            }
        }
        private async void BorrowHistorySearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (BorrowedSearchBox == null || BorrowedGrid == null)
                return;

            if (BorrowedSearchBox.Text == "Search by name or status...")
            {
                await LoadBorrowHistoryRequests(BorrowHistoryGrid);
                return;
            }

            string query = BorrowHistorySearchBox.Text.Trim();
            await LoadBorrowHistoryRequests(BorrowHistoryGrid, query);
        }
        private void BorrowHistoryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        private async Task LoadBorrowHistoryRequests(DataGrid borrowHistoryGrid, string? search = "", string? status = null)
        {
            if (borrowHistoryGrid == null) return;

            var (success, records, message) = await supabaseService.SearchBorrowedStatusAsync(search ?? "", status);

            if (success && records != null)
            {
                borrowHistoryGrid.ItemsSource = records;
            }
            else
            {
                MessageBox.Show($"Failed to load borrow history records: {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void RefreshBorrowHistory_Click(object sender, RoutedEventArgs e)
        {
            BorrowHistorySearchBox.Text = "Search by name or status...";
            BorrowHistorySearchBox.Foreground = Brushes.Gray;

            if (StatusFilter != null)
                StatusFilter.SelectedIndex = 0; 

            await LoadBorrowHistoryRequests(BorrowHistoryGrid);

            await GetBorrowHistoryStats();
        }
        private async void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BorrowHistoryGrid == null || StatusFilter == null)
                return;

            string selectedStatus = (StatusFilter.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (selectedStatus == "All")
            {
                await LoadBorrowHistoryRequests(BorrowHistoryGrid);
            }
            else
            {
                await LoadBorrowHistoryRequests(BorrowHistoryGrid, status: selectedStatus);
            }

            await GetBorrowHistoryStats(); 
        }
        private async Task GetBorrowHistoryStats()
        {
            var (success, allHistory, message) = await supabaseService.SearchBorrowedStatusAsync("", null);
            if (!success || allHistory == null)
            {
                MessageBox.Show($"Failed to load history stats: {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int total = allHistory.Count;

            TotalHistoryCountTextBlock.Text = total.ToString();
            PendingCountTextBlock.Text = allHistory.Count(i => i.Status?.Equals("Pending", StringComparison.OrdinalIgnoreCase) == true).ToString();
            BorrowedCountTextBlock.Text = allHistory.Count(i => i.Status?.Equals("Borrowed", StringComparison.OrdinalIgnoreCase) == true).ToString();
            ReturnedCountTextBlock.Text = allHistory.Count(i => i.Status?.Equals("Returned", StringComparison.OrdinalIgnoreCase) == true).ToString();
            OverdueCountTextBlock.Text = allHistory.Count(i => i.Status?.Equals("Overdue", StringComparison.OrdinalIgnoreCase) == true).ToString();
            LostCountTextBlock.Text = allHistory.Count(i => i.Status?.Equals("Lost", StringComparison.OrdinalIgnoreCase) == true).ToString();
            DamagedCountTextBlock.Text = allHistory.Count(i => i.Status?.Equals("Damaged", StringComparison.OrdinalIgnoreCase) == true).ToString();
            ResolvedCountTextBlock.Text = allHistory.Count(i => i.Status?.Equals("Resolved", StringComparison.OrdinalIgnoreCase) == true).ToString();
            RejectedCountTextBlock.Text = allHistory.Count(i => i.Status?.Equals("Rejected", StringComparison.OrdinalIgnoreCase) == true).ToString();
        }
        
        private List<BorrowRecordDetail> _allProblematicRecords = new();
        private ObservableCollection<BorrowRecordDetail> _filteredRecords = new();
        private async Task LoadProblematicHistoryRequests()
        {
            var statuses = new[] { "Overdue", "Lost", "Damaged" };
            var tempList = new List<BorrowRecordDetail>();

            foreach (var status in statuses)
            {
                var result = await supabaseService.SearchBorrowedStatusAsync("", status);
                if (result.success && result.records != null)
                {
                    foreach (var borrow in result.records)
                    {
                        var (invSuccess, inventoryItem, _) = await supabaseService.GetInventoryItemByIdAsync(borrow.InventoryId);
                        tempList.Add(new BorrowRecordDetail
                        {
                            BorrowStatus = borrow,
                            Inventory = invSuccess ? inventoryItem : null,
                            Admin = this.admin 
                        });
                    }
                }
            }

            _allProblematicRecords = tempList;
            UpdateFilteredRecords(_allProblematicRecords);
            ProblematicBorrowList.ItemsSource = _filteredRecords;
        }
        private void ProblematicSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ProblematicSearchBox.Text == "Search...")
            {
                ProblematicSearchBox.Text = "";
                ProblematicSearchBox.Foreground = Brushes.Black;
            }
        }
        private void ProblematicSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BorrowedSearchBox.Text))
            {
                ProblematicSearchBox.Text = "Search...";
                ProblematicSearchBox.Foreground = Brushes.Gray;
            }
        }
        private void ProblematicSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = ProblematicSearchBox.Text;

            if (string.IsNullOrWhiteSpace(query) || query == "Search...")
            {
                UpdateFilteredRecords(_allProblematicRecords);
            }
            else
            {
                query = query.ToLowerInvariant();
                var filtered = _allProblematicRecords
                    .Where(r => r.GetSearchText().Contains(query))
                    .ToList();

                UpdateFilteredRecords(filtered);
            }
        }
        private void UpdateFilteredRecords(List<BorrowRecordDetail> records)
        {
            _filteredRecords.Clear();
            foreach (var record in records)
            {
                _filteredRecords.Add(record);
            }
        }
        private void ExpandAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ProblematicBorrowList.Items)
            {
                var container = ProblematicBorrowList.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                if (container != null)
                {
                    var control = FindVisualChild<ProblematicBorrowControl>(container);
                    control?.ExpandDetails();
                }
            }
        }
        private void CollapseAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ProblematicBorrowList.Items)
            {
                var container = ProblematicBorrowList.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                if (container != null)
                {
                    var control = FindVisualChild<ProblematicBorrowControl>(container);
                    control?.CollapseDetails();
                }
            }
        }
        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

    }
}

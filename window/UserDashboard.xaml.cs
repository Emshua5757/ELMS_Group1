using ELMS_Group1.database;
using ELMS_Group1.model;
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
    /// Interaction logic for UserDashboard.xaml
    /// </summary>
    public partial class UserDashboard : Window
    {
        private SupabaseService supabaseService = new SupabaseService();
        private List<BorrowRecordDetail> _cachedBorrowDetails = new();
        private User user;
        private bool _isLoading = false;

        public UserDashboard(User user)
        {
            InitializeComponent();
            this.user = user;
            UsernameText.Text = user.FullName;
            MyAccount_Name.Text = user.FullName;
            MyAccount_Email.Text = "Email: " + user.Email;
            MyAccount_StudentID.Text = "Student ID: " + user.IdNumber;
            MyAccount_MemberSince.Text = "Member since: " + user.CreatedAt;
            _ = LoadInventory(InventoryGrid);
            _ = LoadBorrowedStatusAsync(forceRefresh: true);
        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ShowPanel(Border panelToShow)
        {

            InventoryPanel.Visibility = Visibility.Collapsed;
            BorrowedHistoryPanel.Visibility = Visibility.Collapsed;
            PenaltiesPanel.Visibility = Visibility.Collapsed;
            AccountPanel.Visibility = Visibility.Collapsed;


            panelToShow.Visibility = Visibility.Visible;
        }
        private async void ShowInventoryPanel(object sender, RoutedEventArgs e)
        {
            ShowPanel(InventoryPanel);
            await LoadInventory(InventoryGrid);
        }
        private async void ShowBorrowedHistoryPanel(object sender, RoutedEventArgs e)
        {
            ShowPanel(BorrowedHistoryPanel);
            await LoadBorrowedStatusAsync(forceRefresh: true);
        }
        private async void ShowPenaltiesPanel(object sender, RoutedEventArgs e)
        {
            ShowPanel(PenaltiesPanel);
            await LoadBorrowedStatusAsync(forceRefresh: true);
        }
        private void ShowAccountPanel(object sender, RoutedEventArgs e)
        {
            ShowPanel(AccountPanel);
        }
        private async void BorrowItem(object sender, RoutedEventArgs e)
        {
            if (InventoryGrid.SelectedItem is InventoryItem selectedItem)
            {
                var (success, message) = await supabaseService.BorrowRequestItemAsync(selectedItem.Id, user.Id, user.FullName);

                if (success)
                {
                    MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select an item to borrow.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void InventoryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void ShowAboutPanel(object sender, RoutedEventArgs e)
        {
            AboutUs aboutUsWindow = new AboutUs();
            aboutUsWindow.ShowDialog();
        }
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            UserLogin userLoginWindow = new UserLogin();
            userLoginWindow.Show();
            this.Close();
        }
        private async Task LoadInventory(DataGrid inventoryGrid, string? search = null)
        {
            if (StatusFilterComboBox == null)
                return;

            var selectedStatusItem = StatusFilterComboBox.SelectedItem as ComboBoxItem;
            string? statusFilter = selectedStatusItem?.Content.ToString();

            if (statusFilter == "No Filter")
                statusFilter = null;

            (bool success, List<InventoryItem>? items, string message) =
                await supabaseService.SearchInventoryItemsAsync(search ?? "", statusFilter);

            if (inventoryGrid == null)
                return;

            if (success && items != null)
            {
                inventoryGrid.ItemsSource = items;
            }
            else
            {
                MessageBox.Show($"Failed to load inventory items: {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void InventorySearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (InventorySearchBox == null)
                return;

            string searchText = InventorySearchBox.Text == "Search..." ? "" : InventorySearchBox.Text.Trim();

            await LoadInventory(InventoryGrid, searchText);
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
        private async void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string searchText = InventorySearchBox?.Text == "Search..." ? "" : InventorySearchBox?.Text.Trim() ?? "";

            await LoadInventory(InventoryGrid, searchText);
        }
        private async Task LoadBorrowedStatusAsync(bool forceRefresh = false)
        {
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                string statusFilter = GetSelectedStatusFilter();
                string fetchStatusFilter = forceRefresh ? null : statusFilter;

                if (_cachedBorrowDetails.Count == 0 || forceRefresh)
                {
                    var result = await supabaseService.SearchBorrowedStatusAsync("", fetchStatusFilter, null, user.Id);
                    if (!result.success || result.records == null)
                    {
                        MessageBox.Show($"Error loading borrowed status: {result.message}");
                        return;
                    }

                    _cachedBorrowDetails.Clear();

                    foreach (var record in result.records)
                    {
                        InventoryItem? inventory = null;
                        Admin? admin = null;

                        var invRes = await supabaseService.GetInventoryItemByIdAsync(record.InventoryId);
                        if (invRes.success) inventory = invRes.item;

                        if (record.ApprovedBy.HasValue)
                        {
                            var adminRes = await supabaseService.GetAdminByIDAsync(record.ApprovedBy.Value);
                            if (adminRes.success) admin = adminRes.admin;
                        }

                        _cachedBorrowDetails.Add(new BorrowRecordDetail
                        {
                            BorrowStatus = record,
                            Inventory = inventory,
                            Admin = admin
                        });
                    }
                }

                BorrowedHistoryGrid.ItemsSource = null; // clear before setting
                FilterAndDisplay(BorrowedSearchBox.Text, statusFilter);
                LoadPenalties();
                UpdateQuickStats();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void FilterAndDisplay(string searchText, string statusFilter)
        {
            if (BorrowedHistoryGrid == null) return;

            string lowerSearch = searchText == "Search..." ? "" : searchText.ToLower();

            var filtered = _cachedBorrowDetails
                .Where(r =>
                    (string.IsNullOrWhiteSpace(lowerSearch) || r.GetSearchText().Contains(lowerSearch)) &&
                    (string.IsNullOrWhiteSpace(statusFilter) || r.BorrowStatus.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase))
                )
                .ToList();

            BorrowedHistoryGrid.ItemsSource = filtered;
        }
        private string GetSelectedStatusFilter()
        {
            if (StatusFilterHistoryComboBox == null) return "";
            if (StatusFilterHistoryComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selected = selectedItem.Content?.ToString() ?? "";
                return selected == "No Filter" ? "" : selected;
            }
            return "";
        }
        private void BorrowedSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (BorrowedSearchBox.Text == "Search...")
            {
                BorrowedSearchBox.Text = "";
                BorrowedSearchBox.Foreground = Brushes.Black;
            }
        }
        private void BorrowedSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BorrowedSearchBox.Text))
            {
                BorrowedSearchBox.Text = "Search...";
                BorrowedSearchBox.Foreground = Brushes.Gray;
            }
        }
        private void BorrowedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterAndDisplay(BorrowedSearchBox.Text, GetSelectedStatusFilter());
        }
        private void StatusFilterHistoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterAndDisplay(BorrowedSearchBox.Text, GetSelectedStatusFilter());
        }
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadBorrowedStatusAsync(forceRefresh: true);
        }
        private void LoadPenalties()
        {
            if (PenaltiesGrid == null || TotalPenaltiesTextBlock == null) return;

            var penaltyStatuses = new[] { "Lost", "Damaged", "Overdue" };

            var penalties = _cachedBorrowDetails
                .Where(r => penaltyStatuses.Contains(r.BorrowStatus.Status))
                .ToList();

            PenaltiesGrid.ItemsSource = penalties;

            decimal totalPenalty = penalties.Sum(p => p.PenaltyAmount);

            TotalPenaltiesTextBlock.Text = totalPenalty > 0 ? totalPenalty.ToString("C", new System.Globalization.CultureInfo("en-PH")) : "₱0.00";
        }
        private void UpdateQuickStats()
        {
            var borrowed = _cachedBorrowDetails.Where(r => r.BorrowStatus.Status == "Borrowed").Count();
            var overdue = _cachedBorrowDetails.Where(r => r.BorrowStatus.Status == "Overdue").Count();
            var request = _cachedBorrowDetails.Where(r => r.BorrowStatus.Status == "Pending").Count();
            var penalties = _cachedBorrowDetails
                .Where(r => r.BorrowStatus.Status is "Overdue" or "Lost" or "Damaged")
                .Sum(r => r.PenaltyAmount);

            QuickStats_RequestCount.Text = request.ToString();
            QuickStats_BorrowedCount.Text = borrowed.ToString();
            QuickStats_OverdueCount.Text = overdue.ToString();

            Stat_ItemsBorrowed.Text = borrowed.ToString();
            Stat_OverdueItems.Text = overdue.ToString();
            Stat_PendingRequests.Text = request.ToString();
            Stat_TotalPenalties.Text = penalties.ToString("C", new System.Globalization.CultureInfo("en-PH"));
        }
    }
}

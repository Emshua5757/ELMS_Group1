using ELMS_Group1.database;
using ELMS_Group1.model;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ELMS_Group1.window
{
    /// <summary>
    /// Interaction logic for ReturnStatusDialog.xaml
    /// </summary>
    public partial class ReturnStatusDialog : Window
    {
        private readonly BorrowedStatus borrowedStatus;
        private readonly SupabaseService supabaseService = new SupabaseService();

        public string SelectedStatus { get; private set; } = "Returned"; // Default

        public ReturnStatusDialog(BorrowedStatus status)
        {
            InitializeComponent();
            borrowedStatus = status;
            Loaded += ReturnStatusDialog_Loaded;
        }

        private async void ReturnStatusDialog_Loaded(object sender, RoutedEventArgs e)
        {
            txtBorrowerName.Text = borrowedStatus.BorrowerName;
            txtBorrowedDate.Text = borrowedStatus.BorrowedDate.ToLocalTime().ToString("f");
            txtDueDate.Text = borrowedStatus.DueDate?.ToLocalTime().ToString("f") ?? "No due date";

            string itemName = await GetInventoryItemNameByIdAsync(borrowedStatus.InventoryId);
            txtItemName.Text = itemName;
        }

        private async Task<string> GetInventoryItemNameByIdAsync(long inventoryId)
        {
            try
            {
                var (success, item, message) = await supabaseService.GetInventoryItemByIdAsync(inventoryId);
                return success && item != null ? item.Name : $"(Error: {message})";
            }
            catch (Exception ex)
            {
                return $"(Exception: {ex.Message})";
            }
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (StatusComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                SelectedStatus = selectedItem.Content.ToString();
            }

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}

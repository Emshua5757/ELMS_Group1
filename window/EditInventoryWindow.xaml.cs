using ELMS_Group1.model;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for EditInventoryWindow.xaml
    /// </summary>
    public partial class EditInventoryWindow : Window
    {
        public InventoryItem UpdatedInventoryItem { get; private set; }

        public EditInventoryWindow(InventoryItem item)
        {
            InitializeComponent();
            LoadInventoryItem(item);
        }

        private void LoadInventoryItem(InventoryItem item)
        {
            txtName.Text = item.Name;
            txtCategory.Text = item.Category;
            txtDescription.Text = item.Description;
            txtSerialNumber.Text = item.SerialNumber;
            txtLocation.Text = item.Location;
            dpAcquisitionDate.SelectedDate = item.AcquisitionDate ?? DateTime.UtcNow;
            txtValue.Text = item.Value.ToString(CultureInfo.InvariantCulture);

            UpdatedInventoryItem = new InventoryItem
            {
                Id = item.Id,
                CreatedAt = item.CreatedAt,  
                UpdatedAt = item.UpdatedAt,  
                CreatedBy = item.CreatedBy
            };

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs(out decimal value))
            {
                MessageBox.Show("Please correct the highlighted fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UpdatedInventoryItem.Name = txtName.Text.Trim();
            UpdatedInventoryItem.Category = txtCategory.Text.Trim();
            UpdatedInventoryItem.Description = txtDescription.Text.Trim();
            UpdatedInventoryItem.SerialNumber = txtSerialNumber.Text.Trim();
            UpdatedInventoryItem.Location = txtLocation.Text.Trim();
            UpdatedInventoryItem.AcquisitionDate = dpAcquisitionDate.SelectedDate;
            
            UpdatedInventoryItem.Value = value;
            UpdatedInventoryItem.Status = "Available";
            UpdatedInventoryItem.UpdatedAt = DateTime.UtcNow;
            DialogResult = true;
        }

        private bool ValidateInputs(out decimal value)
        {
            value = 0m;
            bool isValid = true;

            var defaultBrush = new SolidColorBrush(Color.FromRgb(204, 204, 204));

            txtName.BorderBrush = defaultBrush;
            txtCategory.BorderBrush = defaultBrush;
            txtSerialNumber.BorderBrush = defaultBrush;
            txtLocation.BorderBrush = defaultBrush;
            txtValue.BorderBrush = defaultBrush;

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                txtName.BorderBrush = Brushes.Red;
                txtName.Focus();
                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(txtCategory.Text))
            {
                txtCategory.BorderBrush = Brushes.Red;
                txtCategory.Focus();
                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(txtSerialNumber.Text))
            {
                txtSerialNumber.BorderBrush = Brushes.Red;
                txtSerialNumber.Focus();
                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(txtLocation.Text))
            {
                txtLocation.BorderBrush = Brushes.Red;
                txtLocation.Focus();
                isValid = false;
            }
            else if (!decimal.TryParse(txtValue.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out value) || value <= 0)
            {
                txtValue.BorderBrush = Brushes.Red;
                txtValue.Focus();
                isValid = false;
            }

            return isValid;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
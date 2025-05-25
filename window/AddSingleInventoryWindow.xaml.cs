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
    /// Interaction logic for AddSingleInventoryWindow.xaml
    /// </summary>
    public partial class AddSingleInventoryWindow : Window
    {
        public InventoryItem InventoryItem { get; private set; }

        public AddSingleInventoryWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private bool ValidateInputs(out decimal value)
        {
            value = 0;
            bool isValid = true;

            // Reset border colors
            txtName.BorderBrush = txtCategory.BorderBrush = txtSerialNumber.BorderBrush =
                txtLocation.BorderBrush = txtValue.BorderBrush = new SolidColorBrush(Color.FromRgb(204, 204, 204)); // #CCC

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

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs(out decimal value))
            {
                MessageBox.Show("Please correct the highlighted fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            InventoryItem = new InventoryItem
            {
                Name = txtName.Text.Trim(),
                Category = txtCategory.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                SerialNumber = txtSerialNumber.Text.Trim(),
                Location = txtLocation.Text.Trim(),
                Status = "Available",
                Value = value,
                AcquisitionDate = dpAcquisitionDate.SelectedDate ?? DateTime.UtcNow,
                CreatedBy = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            DialogResult = true;
        }

    }
}

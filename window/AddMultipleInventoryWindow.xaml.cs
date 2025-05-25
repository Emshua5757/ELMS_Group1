using ELMS_Group1.model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ELMS_Group1.window
{
    public partial class AddMultipleInventoryWindow : Window
    {
        public List<InventoryItem> InventoryItems { get; private set; }

        public AddMultipleInventoryWindow()
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

        private bool ValidateInputs(out decimal value, out int quantity, out int startingNumber)
        {
            value = 0;
            quantity = 0;
            startingNumber = 0;

            bool isValid = true;
            var defaultBrush = new SolidColorBrush(Color.FromRgb(204, 204, 204)); // #CCC

            // Reset all borders
            txtBaseName.BorderBrush = txtCategory.BorderBrush = txtDescription.BorderBrush =
            txtSerialPrefix.BorderBrush = txtLocation.BorderBrush = txtQuantity.BorderBrush =
            txtStartingNumber.BorderBrush = txtValue.BorderBrush = defaultBrush;

            if (string.IsNullOrWhiteSpace(txtBaseName.Text))
            {
                txtBaseName.BorderBrush = Brushes.Red;
                txtBaseName.Focus();
                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(txtCategory.Text))
            {
                txtCategory.BorderBrush = Brushes.Red;
                txtCategory.Focus();
                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(txtSerialPrefix.Text))
            {
                txtSerialPrefix.BorderBrush = Brushes.Red;
                txtSerialPrefix.Focus();
                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(txtLocation.Text))
            {
                txtLocation.BorderBrush = Brushes.Red;
                txtLocation.Focus();
                isValid = false;
            }
            else if (!int.TryParse(txtQuantity.Text, out quantity) || quantity < 1 || quantity > 10)
            {
                txtQuantity.BorderBrush = Brushes.Red;
                txtQuantity.Focus();
                isValid = false;
            }
            else if (!int.TryParse(txtStartingNumber.Text, out startingNumber) || startingNumber < 0)
            {
                txtStartingNumber.BorderBrush = Brushes.Red;
                txtStartingNumber.Focus();
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
            if (!ValidateInputs(out decimal value, out int quantity, out int startingNumber))
            {
                MessageBox.Show("Please correct the highlighted fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime acquisitionDate = dpAcquisitionDate.SelectedDate ?? DateTime.UtcNow;
            InventoryItems = new List<InventoryItem>();

            string baseName = txtBaseName.Text.Trim();
            string serialPrefix = txtSerialPrefix.Text.Trim();
            string category = txtCategory.Text.Trim();
            string description = txtDescription.Text.Trim();
            string location = txtLocation.Text.Trim();

            for (int i = 0; i < quantity; i++)
            {
                int currentNumber = startingNumber + i;

                var item = new InventoryItem
                {
                    Name = $"{baseName} #{currentNumber}",
                    Category = category,
                    Description = description,
                    SerialNumber = $"{serialPrefix}-{currentNumber}",
                    Location = location,
                    Value = value,
                    AcquisitionDate = acquisitionDate,
                    CreatedBy = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = "Available"
                };

                InventoryItems.Add(item);
            }

            DialogResult = true;
        }
    }
}

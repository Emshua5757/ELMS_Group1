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
    /// Interaction logic for AddInventoryOptionDialog.xaml
    /// </summary>
    public partial class AddInventoryOptionDialog : Window
    {
        public enum InventoryAddOption
        {
            None,
            Single,
            Multiple,
            Csv
        }

        public InventoryAddOption SelectedOption { get; private set; } = InventoryAddOption.None;

        public AddInventoryOptionDialog()
        {
            InitializeComponent();
        }

        private void BtnSingle_Click(object sender, RoutedEventArgs e)
        {
            SelectedOption = InventoryAddOption.Single;
            DialogResult = true;
        }

        private void BtnMultiple_Click(object sender, RoutedEventArgs e)
        {
            SelectedOption = InventoryAddOption.Multiple;
            DialogResult = true;
        }

        private void BtnCsv_Click(object sender, RoutedEventArgs e)
        {
            SelectedOption = InventoryAddOption.Csv;
            DialogResult = true;
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
    }
}

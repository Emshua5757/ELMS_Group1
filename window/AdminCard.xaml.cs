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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ELMS_Group1.window
{
    /// <summary>
    /// Interaction logic for AdminCard.xaml
    /// </summary>
    public partial class AdminCard : UserControl
    {
        public AdminCard()
        {
            InitializeComponent();
        }

        public AdminCard(Admin admin) : this()
        {
            DataContext = admin;
        }
    }
}

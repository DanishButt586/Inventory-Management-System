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

namespace Inventory_Management_System_SFML
{
    /// <summary>
    /// Interaction logic for ContactSupport.xaml
    /// </summary>
    public partial class ContactSupport : UserControl
    {
        public ContactSupport()
        {
            InitializeComponent();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text;
            string email = txtEmail.Text;
            string message = txtMessage.Text;

            // Logic to handle the support request (e.g., send an email or save to database)
            MessageBox.Show("Your message has been sent. We will get back to you shortly.", "Support", MessageBoxButton.OK, MessageBoxImage.Information);

            // Optionally, clear the fields after submission
            txtName.Clear();
            txtEmail.Clear();
            txtMessage.Clear();
        }
    }
}

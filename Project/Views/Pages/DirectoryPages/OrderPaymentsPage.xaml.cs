using Project.ViewModels;
using System.Windows.Controls;
using Project.Models;

namespace Project.Views.Pages.DirectoryPages
{
    public partial class OrderPaymentsPage : Page
    {
        public OrderPaymentsPage()
        {
            InitializeComponent();
            this.DataContext = new SomePagesViewModel<Payment>();
        }
    }
}
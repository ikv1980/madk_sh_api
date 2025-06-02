using Project.ViewModels;
using System.Windows.Controls;
using Project.Models;

namespace Project.Views.Pages.DirectoryPages
{
    public partial class OrderDeliveriesPage : Page
    {
        public OrderDeliveriesPage()
        {
            InitializeComponent();
            this.DataContext = new SomePagesViewModel<Delivery>();
        }
    }
}
using Project.ViewModels;
using System.Windows.Controls;
using Project.Models;

namespace Project.Views.Pages.DirectoryPages
{
    public partial class OrderStatusesPage : Page
    {
        public OrderStatusesPage()
        {
            InitializeComponent();
            this.DataContext = new SomePagesViewModel<Status>();
        }
    }
}
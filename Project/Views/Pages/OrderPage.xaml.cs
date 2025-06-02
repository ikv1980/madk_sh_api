using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Project.ViewModels;
using Project.Views.Pages.DirectoryPages.Edit;

namespace Project.Views.Pages
{
    public partial class OrderPage : Page
    {
        public OrderPage()
        {
            InitializeComponent();
            this.DataContext = new OrderPageViewModel();
        }
    }
}
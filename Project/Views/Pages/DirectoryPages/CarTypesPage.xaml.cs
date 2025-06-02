using Project.ViewModels;
using System.Windows.Controls;
using Project.Models;

namespace Project.Views.Pages.DirectoryPages
{
    public partial class CarTypesPage : Page
    {
        public CarTypesPage()
        {
            InitializeComponent();
            this.DataContext = new SomePagesViewModel<CarType>();
        }
    }
}
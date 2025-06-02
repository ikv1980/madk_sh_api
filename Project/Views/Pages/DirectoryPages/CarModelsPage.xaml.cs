using Project.ViewModels;
using System.Windows.Controls;
using Project.Models;

namespace Project.Views.Pages.DirectoryPages
{
    public partial class CarModelsPage : Page
    {
        public CarModelsPage()
        {
            InitializeComponent();
            this.DataContext = new SomePagesViewModel<CarModel>();
        }
    }
}

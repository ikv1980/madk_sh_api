using Project.ViewModels;
using System.Windows.Controls;
using Project.Models;

namespace Project.Views.Pages.DirectoryPages
{
    public partial class CarMarkModelCountries : Page
    {
        public CarMarkModelCountries()
        {
            InitializeComponent();
            this.DataContext = new SomePagesViewModel<CarMarkModelCountry>();
        }
    }
}
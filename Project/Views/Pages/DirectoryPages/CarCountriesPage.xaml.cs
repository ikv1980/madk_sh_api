using Project.ViewModels;
using System.Windows.Controls;
using Project.Models;

namespace Project.Views.Pages.DirectoryPages
{
    public partial class CarCountriesPage : Page
    {
        public CarCountriesPage()
        {
            InitializeComponent();
            this.DataContext = new SomePagesViewModel<CarCountry>();
        }
    }
}
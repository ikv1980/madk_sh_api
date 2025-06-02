using Project.ViewModels;
using System.Windows.Controls;
using Project.Models;

namespace Project.Views.Pages.DirectoryPages
{
    public partial class CarColorsPage : Page
    {
        public CarColorsPage()
        {
            InitializeComponent();
            this.DataContext = new SomePagesViewModel<CarColor>();
        }
    }
}
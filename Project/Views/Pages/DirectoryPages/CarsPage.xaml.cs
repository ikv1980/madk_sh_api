using Project.ViewModels;
using System.Windows.Controls;
using Project.Models;

namespace Project.Views.Pages.DirectoryPages
{
    public partial class CarsPage : Page
    {
        public CarsPage()
        {
            InitializeComponent();
            this.DataContext = new SomePagesViewModel<Car>();
        }
    } 
}
using Project.ViewModels;
using System.Windows.Controls;
using Project.Models;

namespace Project.Views.Pages.DirectoryPages
{
    public partial class CarMarksPage : Page
    {
        public CarMarksPage()
        {
            InitializeComponent();
            this.DataContext = new SomePagesViewModel<CarMark>();
        }
    }
}
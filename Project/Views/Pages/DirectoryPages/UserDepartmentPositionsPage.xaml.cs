using Project.ViewModels;
using System.Windows.Controls;
using Project.Models;

namespace Project.Views.Pages.DirectoryPages
{
    public partial class UserDepartmentPositionsPage : Page
    {
        public UserDepartmentPositionsPage()
        {
            InitializeComponent();
            this.DataContext = new SomePagesViewModel<UserDepartmentPosition>();
        }
    }
}
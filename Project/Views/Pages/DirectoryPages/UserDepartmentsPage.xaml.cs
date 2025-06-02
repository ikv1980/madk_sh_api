using Project.ViewModels;
using System.Windows.Controls;
using Project.Models;

namespace Project.Views.Pages.DirectoryPages
{
    public partial class UserDepartmentsPage: Page
    {
        public UserDepartmentsPage()
        {
            InitializeComponent();
            this.DataContext = new SomePagesViewModel<UserDepartment>();
        }
    }
}
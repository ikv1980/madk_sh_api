using Project.ViewModels;
using System.Windows.Controls;
using Project.Models;

namespace Project.Views.Pages.DirectoryPages
{
    public partial class UserStatusesPage: Page
    {
        public UserStatusesPage()
        {
            InitializeComponent();
            this.DataContext = new SomePagesViewModel<UserStatus>();
        }
    }
}
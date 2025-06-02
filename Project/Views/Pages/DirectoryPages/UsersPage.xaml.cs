using Project.ViewModels;
using System.Windows.Controls;
using Project.Models;

namespace Project.Views.Pages.DirectoryPages
{
    public partial class UsersPage : Page
    {
        public UsersPage()
        {
            InitializeComponent();
            this.DataContext = new SomePagesViewModel<User>();
        }
    } 
}
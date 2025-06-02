using Project.ViewModels;
using System.Windows.Controls;
using Project.Models;

namespace Project.Views.Pages.DirectoryPages
{
    public partial class UserPositionsPage: Page
    {
        public UserPositionsPage()
        {
            InitializeComponent();
            this.DataContext = new SomePagesViewModel<UserPosition>();
        }
    }
}
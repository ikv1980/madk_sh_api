using System.Windows.Controls;
using Project.ViewModels;

namespace Project.Views.Pages
{
    public partial class ReportPage : Page
    {
        public ReportPage()
        {
            InitializeComponent();
            this.DataContext = new ReportPageViewModel();
        }
    }
}
using System.Windows.Controls;
using Group3.Semester3.DesktopClient.Services;
using Group3.Semester3.DesktopClient.ViewHelpers;
using Group3.Semester3.WebApp.Models.Users;

namespace Group3.Semester3.DesktopClient.Views
{
    public partial class UserProfile : UserControl
    {
        
        
        public UserProfile(ApiService apiService, Switcher switcher)
        {
            InitializeComponent();
            
            userName.Content += apiService.User.Name + "!";
            userEmail.Content += apiService.User.Email;
            userId.Content += apiService.User.Id.ToString();
        }
    }
}
using System.Windows.Controls;
using Group3.Semester3.DesktopClient.Services;
using Group3.Semester3.WebApp.Models.Users;

namespace Group3.Semester3.DesktopClient.Views
{
    public partial class UserProfile : UserControl
    {
        
        
        public UserProfile(UserModel currentUser)
        {
            InitializeComponent();
            
            userName.Content += currentUser.Name + "!";
            userEmail.Content += currentUser.Email;
            userId.Content += currentUser.Id.ToString();
        }
    }
}
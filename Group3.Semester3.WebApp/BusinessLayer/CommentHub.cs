using System.Threading.Tasks;
using Group3.Semester3.WebApp.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Group3.Semester3.WebApp.BusinessLayer
{
    
    [Authorize(AuthenticationSchemes = (CookieAuthenticationDefaults.AuthenticationScheme + "," + JwtBearerDefaults.AuthenticationScheme))]
    public class CommentHub : Hub
    {
        private ICommentService _commentService;
        private IUserService _userService;
        
        public CommentHub(ICommentService commentService, IUserService userService)
        {
            _commentService = commentService;
            _userService = userService;
        }
        
        public async Task NewComment(Comment comment)
        {
            var user = _userService.GetFromHttpContext(Context);

            comment = _commentService.CreateComment(user, comment);
            
            await Clients.All.SendAsync("NewComment", comment);
        }
    }
}
using System;
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
        
        public async Task NewComment(Comment newComment)
        {
            var user = _userService.GetFromHttpContext(Context);

            var comment = _commentService.CreateComment(user, newComment);
            var fileId = comment.FileId.ToString();
            
            await Clients.Group(fileId).SendAsync("NewComment", comment, fileId, user.Name);
        }

        public async Task AddToGroup(Guid fileId)
        {
            var user = _userService.GetFromHttpContext(Context);
            if (_commentService.AddToGroup(user, fileId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, fileId.ToString());
            }
        }
    }
}
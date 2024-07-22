using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace TMP.Application.Hubs
{
    public class CommentHub : Hub
    {
        public async Task SendComment(string content, int taskId)
        {
            // Create a comment object with the content and taskId
            var comment = new
            {
                Content = content,
                TaskId = taskId
            };

            // Broadcast the comment to all connected clients
            await Clients.All.SendAsync("ReceiveComment", comment);
        }
    }
}

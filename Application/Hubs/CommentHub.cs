using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace TMP.Application.Hubs
{
    public class CommentHub : Hub
    {
        public async Task SendComment(string content, int taskId, string projectId)
        {
            var comment = new
            {
                Content = content,
                TaskId = taskId
            };

            await Clients.Group(projectId).SendAsync("ReceiveComment", comment);
        }

        public async Task JoinProjectGroup(string projectId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, projectId);
        }

        public async Task LeaveProjectGroup(string projectId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, projectId);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.SignalR;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Concurrent;

namespace TMPApplication.Hubs
{
    
    public class NotificationHub : Hub
    {

        private static readonly ConcurrentDictionary<string, string> _userConnections = new(); //Thread safe dictionary

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                
                _userConnections[Context.ConnectionId] = userId;
                Console.WriteLine($"User {userId} connected with connection ID {Context.ConnectionId}");
            }
            
            await base.OnConnectedAsync();
        }

       

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _userConnections.TryRemove(Context.ConnectionId, out var userId);

            return base.OnDisconnectedAsync(exception);
        }

        
    }
}

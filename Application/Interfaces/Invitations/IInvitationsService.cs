using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPApplication.Interfaces.Invitations
{
    public interface IInvitationsService
    {
        Task CreateInvitationAsync(int projectId, string email);
        Task AcceptInvitationAsync(string token);
    }
}

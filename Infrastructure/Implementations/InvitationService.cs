using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TMP.Application.DTOs.ProjectUserDtos;
using TMP.Application.Interfaces;
using TMPApplication.Interfaces;
using TMPApplication.Interfaces.Invitations;
using TMPApplication.Interfaces.Projects;
using TMPDomain.Entities;
using Task = System.Threading.Tasks.Task;

namespace TMPInfrastructure.Implementations
{
    public class InvitationService : IInvitationsService
    {
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProjectService _projectService;
        private readonly IMapper _mapper;

        public InvitationService(IEmailService emailService, IUnitOfWork unitOfWork, IProjectService projectService, IMapper mapper)
        {
            _projectService = projectService;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task CreateInvitationAsync(int projectId, string email)
        {
            if (!IsValidEmail(email))
                throw new Exception("Email is not valid");

            var token = Guid.NewGuid().ToString(); // Generate a unique token
            var invitation = new Invitation
            {
                ProjectId = projectId,
                Email = email,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(3),
                IsAccepted = false
            };

            _unitOfWork.Repository<Invitation>().Create(invitation);
            await _unitOfWork.Repository<Invitation>().SaveChangesAsync();
            var invitationLink = $"url?token={token}";
            var emailBody = $"You have been invited to join a project. Click <a href='{invitationLink}'>here</a> to accept the invitation.";
            await _emailService.SendEmail(email, "Project Invitation", emailBody);
        }

        private async Task<Invitation> GetInvitationByTokenAsync(string token)
        {
            return await _unitOfWork.Repository<Invitation>().GetByCondition(i => i.Token == token && !i.IsAccepted && i.ExpiresAt > DateTime.UtcNow).FirstOrDefaultAsync();
        }

        public async Task<bool> AcceptInvitationAsync(string token)
        {
            var invitation = await GetInvitationByTokenAsync(token);
            if (invitation == null)
                throw new Exception("Invalid or expired invitation.");

            invitation.IsAccepted = true;
            _unitOfWork.Repository<Invitation>().Update(invitation);
            await _unitOfWork.Repository<Invitation>().SaveChangesAsync();
            var userToAdd = await _unitOfWork.Repository<User>().GetByCondition(x => x.Email == invitation.Email).FirstOrDefaultAsync();

            var projectUsers = new AddProjectUserDto
            {
                ProjectId = invitation.ProjectId,
                UserId = userToAdd.Id,
            };


            var isUserAdded = await _projectService.AddUserToProjectAsync(projectUsers, userToAdd.Id);

            return isUserAdded;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);  //parse the provided email string
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }
}

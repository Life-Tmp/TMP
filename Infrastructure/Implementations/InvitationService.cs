using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<InvitationService> _logger;

        public InvitationService(IEmailService emailService, IUnitOfWork unitOfWork, IProjectService projectService, IMapper mapper, ILogger<InvitationService> logger)
        {
            _projectService = projectService;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        #region Create
        public async Task CreateInvitationAsync(int projectId, string email)
        {
            _logger.LogInformation("Creating invitation for project ID: {ProjectId} and email: {Email}", projectId, email);

            if (!IsValidEmail(email))
            {
                _logger.LogWarning("Invalid email address: {Email}", email);
                throw new Exception("Email is not valid");
            }

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

            _logger.LogInformation("Invitation created and email sent to: {Email}", email);
        }
        #endregion

        #region Read
        public async Task<bool> AcceptInvitationAsync(string token)
        {
            _logger.LogInformation("Accepting invitation with token: {Token}", token);

            var invitation = await GetInvitationByTokenAsync(token);
            if (invitation == null)
            {
                _logger.LogWarning("Invalid or expired invitation token: {Token}", token);
                throw new Exception("Invalid or expired invitation.");
            }

            invitation.IsAccepted = true;
            _unitOfWork.Repository<Invitation>().Update(invitation);
            await _unitOfWork.Repository<Invitation>().SaveChangesAsync();

            var userToAdd = await _unitOfWork.Repository<User>()
                .GetByCondition(x => x.Email == invitation.Email)
                .FirstOrDefaultAsync();

            if (userToAdd == null)
            {
                _logger.LogWarning("User with email: {Email} not found", invitation.Email);
                throw new Exception("User not found.");
            }

            var projectUsers = new AddProjectUserDto
            {
                ProjectId = invitation.ProjectId,
                UserId = userToAdd.Id,
            };

            var isUserAdded = await _projectService.AddUserToProjectAsync(projectUsers, userToAdd.Id);

            if (isUserAdded)
            {
                _logger.LogInformation("User with email: {Email} successfully added to project ID: {ProjectId}", invitation.Email, invitation.ProjectId);
            }
            else
            {
                _logger.LogWarning("Failed to add user with email: {Email} to project ID: {ProjectId}", invitation.Email, invitation.ProjectId);
            }

            return isUserAdded;
        }

        private async Task<Invitation> GetInvitationByTokenAsync(string token)
        {
            _logger.LogInformation("Fetching invitation with token: {Token}", token);

            var invitation = await _unitOfWork.Repository<Invitation>()
                .GetByCondition(i => i.Token == token && !i.IsAccepted && i.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (invitation == null)
            {
                _logger.LogWarning("Invitation with token: {Token} not found or expired", token);
            }

            return invitation;
        }
        #endregion

        #region Update
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
        #endregion
    }
}

using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Task = System.Threading.Tasks.Task;
using TMP.Application.Interfaces;
using TMPApplication.Interfaces;
using TMPApplication.Interfaces.Invitations;
using TMPDomain.Entities;
using Microsoft.EntityFrameworkCore;
using TMP.Application.DTOs.ProjectUserDtos;
using TMPDomain.Enumerations;
using TMPApplication.Interfaces.Projects;
using AutoMapper;

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
            var token = Guid.NewGuid().ToString(); // Generate a unique token
            var invitation = new Invitation
            {
                ProjectId = projectId,
                Email = email,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7), 
                IsAccepted = false
            };

            _unitOfWork.Repository<Invitation>().Create(invitation);
            await _unitOfWork.Repository<Invitation>().SaveChangesAsync();
            // Send the email
            var invitationLink = $"https://localhost:7001/api/invite/accept?token={token}";
            var emailBody = $"You have been invited to join a project. Click <a href='{invitationLink}'>here</a> to accept the invitation.";
            await _emailService.SendEmailInvite(email, "Project Invitation", emailBody);
        }

        private async Task<Invitation> GetInvitationByTokenAsync(string token)
        {
            return _unitOfWork.Repository<Invitation>().GetByCondition(i => i.Token == token && !i.IsAccepted && i.ExpiresAt > DateTime.UtcNow).FirstOrDefault();
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

            

            var done = await _projectService.AddUserToProjectAsync(projectUsers, userToAdd.Id);

            return done;
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

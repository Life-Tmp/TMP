﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TMP.Application.Interfaces;
using TMPDomain.Entities;
namespace TMP.Service.Helpers
{
    public static class StartupHelper
    {
        public static void RegisterAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = configuration["AuthoritySettings:Authority"];
                options.RequireHttpsMetadata = true;
                options.Audience = configuration["AuthoritySettings:Scope"];
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    NameClaimType = ClaimTypes.Name, 
                    RoleClaimType = ClaimTypes.Role,
                    ValidIssuer = configuration["AuthoritySettings:Authority"],
                    ValidAudience = configuration["AuthoritySettings:Scope"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("4a9db740-2460-471a-b3a1-6d86bb99b279")),
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/notificationHub")))
                        {
                            context.Token = accessToken;
                        }
                        return System.Threading.Tasks.Task.CompletedTask;
                    }
                };
                

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        context.HttpContext.User = context.Principal ?? new ClaimsPrincipal();

                        var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        var firstName = context.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                        var lastName = context.HttpContext.User.FindFirst(ClaimTypes.Surname)?.Value;
                        var email = context.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
                        var user = context.HttpContext.User.FindFirstValue(ClaimTypes.Role);
                        var birthdate = context.HttpContext.User.FindFirst(ClaimTypes.DateOfBirth)?.Value;
                        var phoneNumber = context.HttpContext.User.FindFirst(ClaimTypes.MobilePhone)?.Value;


                        //DateTime birthdateParsed = DateTime.Parse(birthdate);

                        var userService = context.HttpContext.RequestServices.GetService<IUnitOfWork>();

                        var existingUser = userService.Repository<User>().GetById(x => x.Id == userId).FirstOrDefault();

                        if (existingUser == null)
                        {
                            var userToBeAdded = new User
                            {
                                Id = userId,
                                FirstName = firstName ?? "",
                                LastName = lastName ?? "",
                                Email = email,
                                PasswordHash = "",
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                            };

                            userService.Repository<User>().Create(userToBeAdded);
                        }

                        var roles = context.Principal.FindAll("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/roles")
                                            .Select(roleClaim => roleClaim.Value)
                                            .ToList();
                        var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                        if (roles.Any() && claimsIdentity != null)
                        {
                            foreach (var role in roles)
                            {
                                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                            }
                        }

                        userService.Complete();
                    }
                };
                
                options.ForwardDefaultSelector = Selector.ForwardReferenceToken("token");
            });
        }
    }
}

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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
                    NameClaimType = "https://example.com/first_name",
                    RoleClaimType = "https://example.com//roles",
                    ValidIssuer = configuration["AuthoritySettings:Authority"],
                    ValidAudience = configuration["AuthoritySettings:Scope"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("4a9db740-2460-471a-b3a1-6d86bb99b279")),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        context.HttpContext.User = context.Principal ?? new ClaimsPrincipal();

                        var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        var firstName = context.HttpContext.User.FindFirst(ClaimTypes.GivenName)?.Value;
                        var lastName = context.HttpContext.User.FindFirst(ClaimTypes.Surname)?.Value;
                        var email = context.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
                        //var gender = context.HttpContext.User.FindFirst(ClaimTypes.Gender)?.Value;
                        //var birthdate = context.HttpContext.User.FindFirst(ClaimTypes.DateOfBirth)?.Value;
                        //var phoneNumber = context.HttpContext.User.FindFirst("phone_number")?.Value;


                        //DateTime birthdateParsed = DateTime.Parse(birthdate);

                        var userService = context.HttpContext.RequestServices.GetService<IUnitOfWork>();

                        var existingUser = userService.Repository<User>().GetById(x => x.Id == userId).FirstOrDefault();

                        if (existingUser == null)
                        {
                            var userToBeAdded = new User
                            {
                                Id = userId,
                                FirstName = firstName,
                                LastName = lastName,
                                Email = email,
                                //Gender = gender,
                                //PhoneNumber = phoneNumber ?? " ",
                                //DateOfBirth = DateOnly.FromDateTime(DateTime.Now)
                            };

                            userService.Repository<User>().Create(userToBeAdded);

                            //var emailService = context.HttpContext.RequestServices.GetService<IEmailSender>();
                            //var hostEnvironment = context.HttpContext.RequestServices.GetService<IWebHostEnvironment>();

                            //if (emailService != null)
                            //{
                            //    var subject = "Welcome to Life TMP";
                            //    var htmlBody = string.Empty;

                            //    using (StreamReader streamReader = File.OpenText("Templates/EmailTemplate.html"))
                            //    {
                            //        htmlBody = streamReader.ReadToEnd();
                            //    }

                            //    string messageBody = string.Format(htmlBody, "Welcome to TMP",
                            //        "Greetings",
                            //        userToBeAdded.FirstName,
                            //        "An account has been created for you with the following email:",
                            //        userToBeAdded.EmailAddress,
                            //        "Best regards,",
                            //        "Life Team");

                            //    emailService.SendEmailAsync(userToBeAdded.EmailAddress, subject, messageBody);
                            //}
                        }
                        else
                        {
                            existingUser.FirstName = firstName;
                            existingUser.LastName = lastName;
                            // existingUser.PhoneNumber = phoneNumber;

                            userService.Repository<User>().Update(existingUser);
                        }

                        userService.Complete();


                    }
                };
                
                options.ForwardDefaultSelector = Selector.ForwardReferenceToken("token");
            });
        }
    }
}

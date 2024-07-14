
using AutoMapper;
using Amazon.S3;

using Microsoft.OpenApi.Models;
using System.Runtime.Loader;
using TMP.Application.Interfaces;
using TMP.Persistence;
using TMP.Service.Helpers;
using TMPApplication.MapperProfiles;
using TMPApplication.AttachmentTasks;
using TMPInfrastructure.Implementations;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3.Transfer;
using Amazon;
using Amazon.Runtime;
using TMPApplication.UserTasks;
namespace TMP.Service;

class Program
{
    static void Main(string[] args)
    {
        var files = Directory.GetFiles(
            AppDomain.CurrentDomain.BaseDirectory,
            "TMP*.dll");

        var assemblies = files
            .Select(p => AssemblyLoadContext.Default.LoadFromAssemblyPath(p));

        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true); //CHECK:
        builder.Configuration.AddEnvironmentVariables();
        
        builder.Services.RegisterAuthentication(builder.Configuration);
        

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "TMP", Version = "v1" });
            c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri("https://dev-pt8z60gtcfp46ip0.us.auth0.com/authorize"),
                        TokenUrl = new Uri("https://dev-pt8z60gtcfp46ip0.us.auth0.com/oauth/token"),
                        Scopes = new Dictionary<string, string> { { "TMP", "Access to admin functionalities in TMP" },
                                                                  { "admin", "Access to administrative functions" } }
                    }
                }
            });
            c.DocumentFilter<LowercaseDocumentFilter>();
            c.OperationFilter<AuthorizeCheckOperationFilter>();
            c.MapType<IFormFile>(() => new OpenApiSchema { Type = "file" });
        }
);
       

        builder.Services.AddAdvancedDependencyInjection();

        builder.Services.Scan(p => p.FromAssemblies(assemblies)
            .AddClasses()
            .AsMatchingInterface());
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddDbContext<DatabaseService>();
        builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();
        //var mapperConfiguration = new MapperConfiguration(
        //                mc => mc.AddProfile(new AttachmentMappingProfile())); //TODO: Try to find better way to add profiles

        //IMapper mapper = mapperConfiguration.CreateMapper();

        builder.Services.AddAutoMapper(assemblies); //CHECK: I think this is better, just need to TEST
        var awsOptions = new AWSOptions
        {
            Region = RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"]),
            Credentials = new BasicAWSCredentials(
                    builder.Configuration["AWS:AccessKeyId"],
                    builder.Configuration["AWS:SecretAccessKey"]
                )
        };

        builder.Services.AddDefaultAWSOptions(awsOptions);
        builder.Services.AddAWSService<IAmazonS3>();

        builder.Services.AddScoped<IAttachmentService, AttachmentService>();
        builder.Services.AddScoped<IUserService, UserService>();
        
        var app = builder.Build();


        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.DisplayRequestDuration();
                c.DefaultModelExpandDepth(0);
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TMP-test");
                c.OAuthClientId(builder.Configuration["AuthoritySettings:ClientId"]);
                c.OAuthClientSecret(builder.Configuration["AuthoritySettings:ClientSecret"]);
                c.OAuthAppName("TMP");
                c.OAuthUsePkce();
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            });
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.UseAdvancedDependencyInjection();

        app.Run();
    }
}
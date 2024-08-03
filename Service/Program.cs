using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Elasticsearch.Net;
using Hangfire;
using Microsoft.OpenApi.Models;
using Nest;
using Serilog;
using StackExchange.Redis;
using System.Runtime.Loader;
using TMP.Application.Comments;
using TMP.Application.Hubs;
using TMP.Application.Interfaces;
using TMP.Application.Interfaces.Tags;
using TMP.Infrastructure.Implementations;
using TMP.Infrastructure.Implementations.Tags;
using TMP.Persistence;
using TMP.Service.Helpers;
using TMPApplication.AttachmentTasks;
using TMPApplication.Hubs;
using TMPApplication.Interfaces;
using TMPApplication.Interfaces.Invitations;
using TMPApplication.Interfaces.Reminders;
using TMPApplication.Interfaces.Subtasks;
using TMPApplication.Notifications;
using TMPApplication.UserTasks;
using TMPInfrastructure.Implementations;
using TMPInfrastructure.Implementations.Notifications;
using TMPInfrastructure.Implementations.Reminders;
using TMPInfrastructure.Implementations.Subtasks;
using TMPApplication.Hubs;
using TMPApplication.Interfaces.Invitations;
using FluentValidation;
using TMPDomain.Validations;
using FluentValidation.AspNetCore;
using TMPInfrastructure.Messaging;
using TMPApplication.Interfaces.ContactForms;
using TMP.Infrastructure.Implementations.ContactForms;

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

        // Configure FluentValidation services
        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddFluentValidationClientsideAdapters();

        // Register validators
        builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();
        builder.Services.AddEndpointsApiExplorer();

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
                        Scopes = new Dictionary<string, string> { { "TMP", "TMP" },
                                                                  { "admin", "admin" },
                            { "openid","openid"},{"profile","profile" },{"email","email" } }
                    }
                }
            });
            c.DocumentFilter<LowercaseDocumentFilter>();
            c.OperationFilter<AuthorizeCheckOperationFilter>();
            c.MapType<IFormFile>(() => new OpenApiSchema { Type = "file" });
        }
);
      
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins",
                builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                }
                );
        });
        
        
        builder.Services.AddAdvancedDependencyInjection();
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger);
        builder.Services.Scan(p => p.FromAssemblies(assemblies)
            .AddClasses()
            .AsMatchingInterface());
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        });
        builder.Services.AddDbContext<DatabaseService>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddSingleton<ICacheService, CacheService>();
        builder.Services.AddScoped(typeof(ISearchService<>), typeof(SearchService<>));



        builder.Services.AddAutoMapper(assemblies); //CHECK: I think this is better, just need to TEST
        var awsOptions = new AWSOptions
        {
            Region = RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"]),
            Credentials = new BasicAWSCredentials(
                    builder.Configuration["AWS:AccessKeyId"],
                    builder.Configuration["AWS:SecretAccessKey"]
                )
        };

        #region Elastic
        var settings = new ConnectionSettings(new Uri(builder.Configuration["Elasticsearch:Url"]))
            .BasicAuthentication(builder.Configuration["Elasticsearch:Username"], builder.Configuration["Elasticsearch:Password"])
            .DefaultIndex("users");

        var client = new ElasticClient(settings);
        builder.Services.AddSingleton<IElasticClient>(client);


        #endregion
        #region Redis

        var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
        builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

        #endregion

        builder.Services.AddHttpClient(); //TODO: Chechk again

        builder.Services.AddDefaultAWSOptions(awsOptions);
        builder.Services.AddAWSService<IAmazonS3>();

        builder.Services.AddScoped<IAttachmentService, AttachmentService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddTransient<INotificationService, NotificationService>();
        builder.Services.AddScoped<IInvitationsService, InvitationService>();
        builder.Services.AddTransient<IEmailService, EmailService>();
        builder.Services.AddScoped<IContactFormService, ContactFormService>();
        builder.Services.AddScoped<ICommentService, CommentService>();
        builder.Services.AddScoped<ITagService, TagService>();
        builder.Services.AddScoped<ISubtaskService, SubtaskService>();



        #region Hosted
        builder.Services.AddSingleton<RabbitMQService>();
        builder.Services.AddScoped<MessageHandler>();
        builder.Services.AddScoped<RabbitMQConsumer>(sp =>
        {
            var rabbitMQService = sp.GetRequiredService<RabbitMQService>();
            var serviceScopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            var logger = sp.GetRequiredService<ILogger<RabbitMQConsumer>>();
            var channel = rabbitMQService.GetChannel();
            return new RabbitMQConsumer(channel, logger, serviceScopeFactory);
        });
        #endregion
        builder.Services.AddHostedService<ConsumerHostedService>();

        builder.Services.AddHangfire(configuration => configuration
            .UseSqlServerStorage(builder.Configuration["ConnectionStrings:DefaultConnection"])); // Use your storage configuration

        builder.Services.AddHangfireServer();

        // Add other services
        builder.Services.AddScoped<IReminderService, ReminderService>();

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
        app.UseRouting();
        app.UseHangfireDashboard();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors("AllowAllOrigins");
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHub<CommentHub>("/commentHub"); // Map the SignalR hub
            endpoints.MapHub<NotificationHub>("/notificationHub");
            endpoints.MapHangfireDashboard();
        });
        

        app.MapControllers();

        app.UseAdvancedDependencyInjection();

        app.Run();
    }
}
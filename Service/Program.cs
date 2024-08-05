using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Microsoft.OpenApi.Models;
using Nest;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using StackExchange.Redis;
using System.Reflection;
using System.Runtime.Loader;
using TMP.Application.Comments;
using TMP.Application.Hubs;
using TMP.Application.Interfaces;
using TMP.Application.Interfaces.Tags;
using TMP.Infrastructure.Implementations;
using TMP.Infrastructure.Implementations.ContactForms;
using TMP.Infrastructure.Implementations.Tags;
using TMP.Persistence;
using TMP.Service.Helpers;
using TMPApplication.AttachmentTasks;
using TMPApplication.Hubs;
using TMPApplication.Interfaces;
using TMPApplication.Interfaces.ContactForms;
using TMPApplication.Interfaces.Invitations;
using TMPApplication.Interfaces.Reminders;
using TMPApplication.Interfaces.Subtasks;
using TMPApplication.Notifications;
using TMPApplication.UserTasks;
using TMPDomain.Entities;
using TMPDomain.Validations;
using TMPInfrastructure.Implementations;
using TMPInfrastructure.Implementations.Notifications;
using TMPInfrastructure.Implementations.Reminders;
using TMPInfrastructure.Implementations.Subtasks;
using TMPInfrastructure.Messaging;
using Attachment = TMPDomain.Entities.Attachment;

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

        builder.Services.AddTransient<IValidator<Project>, ProjectValidator>();
        builder.Services.AddTransient<IValidator<Column>, ColumnValidator>();
        builder.Services.AddTransient<IValidator<Comment>, CommentValidator>();
        builder.Services.AddTransient<IValidator<ContactForm>, ContactFormValidator>();
        builder.Services.AddTransient<IValidator<Notification>, NotificationValidator>();
        builder.Services.AddTransient<IValidator<ProjectTeam>, ProjectTeamValidator>();
        builder.Services.AddTransient<IValidator<ProjectUser>, ProjectUserValidator>();
        builder.Services.AddTransient<IValidator<Reminder>, ReminderValidator>();
        builder.Services.AddTransient<IValidator<Subtask>, SubtaskValidator>(); 
        builder.Services.AddTransient<IValidator<Tag>, TagValidator>();
        builder.Services.AddTransient<IValidator<TaskDuration>, TaskDurationValidator>();
        builder.Services.AddTransient<IValidator<TMPDomain.Entities.Task>, TaskValidator>();
        builder.Services.AddTransient<IValidator<Attachment>, AttachmentValidator>();
        builder.Services.AddTransient<IValidator<TeamMember>, TeamMemberValidator>();
        builder.Services.AddTransient<IValidator<Team>, TeamValidator>();
        builder.Services.AddTransient<IValidator<User>, UserValidator>();
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
                        AuthorizationUrl = new Uri(builder.Configuration["AuthoritySettings:AuthorizationEndpoint"]),
                        TokenUrl = new Uri(builder.Configuration["AuthoritySettings:TokenEndpoint"]),
                        Scopes = new Dictionary<string, string> { { "TMP", "TMP" },
                                                                  { "admin", "admin" },
                            { "openid","openid"},{"profile","profile" },{"email","email" } }
                    }
                }
            });
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
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

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(builder.Configuration["Elasticsearch:Url"]))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"logstash-{DateTime.UtcNow:yyyy.MM.dd}",
                ModifyConnectionSettings = x => x.BasicAuthentication(builder.Configuration["Elasticsearch:Username"], builder.Configuration["Elasticsearch:Password"])
            })
            .WriteTo.MSSqlServer(
                connectionString: builder.Configuration["ConnectionStrings:DefaultConnection"],
                sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions { TableName = "Logs", AutoCreateSqlTable = true })
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);

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



        builder.Services.AddAutoMapper(assemblies);
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

        builder.Services.AddHttpClient();

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
        builder.Services.AddScoped<IReminderService, ReminderService>();


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
            .UseSqlServerStorage(builder.Configuration["ConnectionStrings:DefaultConnection"]));

        builder.Services.AddHangfireServer();


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
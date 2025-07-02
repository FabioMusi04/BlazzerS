using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Models;
using ReportingPortalServer.Services;
using ReportingPortalServer.Services.AppwriteIO;
using ReportingPortalServer.Services.Jobs;

namespace ReportingPortalServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string origins = "origins";
            string? frontAddress = builder.Configuration["FrontAddress"];

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(origins, policy =>
                {
                    policy.WithOrigins(frontAddress ?? string.Empty)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });


            builder.Services.AddAuthorization();
            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Your API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token: Bearer {your token}"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }});
            });


            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.UseNetTopologySuite()));

            builder.Services.AddOpenApi();

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.Configure<AppwriteSettings>(builder.Configuration.GetSection("AppwriteSettings"));

            builder.Services.AddHostedService<JobSchedulerService>();
            builder.Services.AddSingleton<IAppwriteClient, AppwriteClient>();
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<ICategoryService, CategoryService>();
            builder.Services.AddSingleton<IUploadFileService, UploadFileService>();
            builder.Services.AddSingleton<IReportReplyService, ReportReplyService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<ITokenVerificationService, TokenVerificationService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IReportReplyService, ReportReplyService>();

            builder.Services.AddScoped<IScheduledJob, NotificationEmailSender>();
            builder.Services.AddScoped<IScheduledJob, NotificationPushSender>();
            builder.Services.AddScoped<IScheduledJob, PendingReportReminderJob>();


            var app = builder.Build();

            app.UseCors("origins");

            //if (app.Environment.IsDevelopment())
            //{
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI();
            //}

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

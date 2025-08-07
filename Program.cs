using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StackOverFlowClone.Data;
using StackOverFlowClone.Jobs.CleanerRefreshToken;
using StackOverFlowClone.Middleware;
using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Models.Role;
using StackOverFlowClone.Services.Implementations;
using StackOverFlowClone.Services.Interfaces;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
namespace StackOverFlowClone
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
    

            // Add services to the container.
            builder.Services.AddControllers();

            // Database context
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


            builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddHangfireServer();


            builder.Services.AddSignalR();



            //Permissions for Normal user and Moderator and Admin
            builder.Services.AddAuthorization(options =>
            {
                var permissions = typeof(Permissions)
                    .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                    .Select(fi => fi.GetRawConstantValue()!.ToString());

                foreach (var permission in permissions)
                {
                    options.AddPolicy(permission, policy =>
                        policy.RequireClaim("permission", permission));
                }
            });
            /*    builder.Services.AddAuthorization(options =>
                {
                    options.AddPolicy("CanAskQuestion", policy =>
                        policy.RequireClaim("permission", "CanAskQuestion"));

                    options.AddPolicy("CanAnswerQuestion", policy =>
                        policy.RequireClaim("permission", "CanAnswerQuestion"));

                    options.AddPolicy("CanComment", policy =>
                        policy.RequireClaim("permission", "CanComment"));

                    options.AddPolicy("CanVote", policy =>
                        policy.RequireClaim("permission", "CanVote"));

                    options.AddPolicy("CanEditAnyPost", policy =>
                        policy.RequireClaim("permission", "CanEditAnyPost"));

                    options.AddPolicy("CanDeleteAnyPost", policy =>
                        policy.RequireClaim("permission", "CanDeleteAnyPost"));

                    options.AddPolicy("CanModerate", policy =>
                        policy.RequireClaim("permission", "CanModerate"));

                    options.AddPolicy("CanManageTags", policy =>
                        policy.RequireClaim("permission", "CanManageTags"));

                    options.AddPolicy("CanViewAnalytics", policy =>
                        policy.RequireClaim("permission", "CanViewAnalytics"));

                    options.AddPolicy("CanManagePermissions", policy =>
                        policy.RequireClaim("permission", "CanManagePermissions"));

                    options.AddPolicy("CanAccessAdminPanel", policy =>
                        policy.RequireClaim("permission", "CanAccessAdminPanel"));

                    options.AddPolicy("CanEditOwnPost", policy =>
                        policy.RequireClaim("permission", "CanEditOwnPost"));

                    options.AddPolicy("CanDeleteOwnPost", policy =>
                        policy.RequireClaim("permission", "CanDeleteOwnPost"));
                    options.AddPolicy("CanDeleteOwnComment", policy =>
                        policy.RequireClaim("permission", "CanDeleteOwnComment"));
                    options.AddPolicy("CanEditOwnComment", policy =>
                        policy.RequireClaim("permission", "CanEditOwnComment"));
                    options.AddPolicy("UpdateUserProfile",policy =>
                        policy.RequireClaim("permission", "UpdateUserProfile"));
                    options.AddPolicy("ChangeOwnPassword", policy =>
                        policy.RequireClaim("permission", "ChangeOwnPassword"));
                    options.AddPolicy("CanBanUser", policy =>
                        policy.RequireClaim("permission", "CanBanUser"));
                    options.AddPolicy("CanBookMark", policy =>
                        policy.RequireClaim("permission", "CanBookMark"));

                });*/
            /*            string[] AllPermissions =
                                         {
                                         //  Moderator Permissions
                                            "CanEditAnyQuestion",
                                            "CanDeleteAnyQuestion",
                                            "CanEditAnyAnswer",
                                            "CanDeleteAnyAnswer",
                                            "CanDeleteComment",
                                            "CanModerateChat",
                                            "CanViewReports",
                                            "CanWarnUsers",

                                            //  Admin Permissions
                                            "CanDeleteUser",
                                            "CanBanUser",
                                            "CanManageRoles",
                                            "CanManagePermissions",
                                            "CanConfigureSystem",
                                            "CanViewSystemLogs",

                                             // Future Features
                                            "CanPinQuestion",
                                            "CanFeatureQuestion",
                                            "CanUseAdvancedSearch",
                                            "CanManageTags",
                                            "CanResetUserPassword",
                                            "CanAccessAnalytics",
                                            "CanSendSystemNotification"

                                         };


                        builder.Services.AddAuthorization(options =>
                        {
                            foreach (var permission in AllPermissions)
                            {
                                options.AddPolicy(permission, policy => policy.RequireClaim("Permission", permission));
                            }
                        });*/

            /*              builder.Services.AddAuthorization(options =>
              {
                  options.AddPolicy("CanEditAnyQuestion", policy => policy.RequireClaim("Permission", "CanEditAnyQuestion"));
                  options.AddPolicy("CanDeleteAnyQuestion", policy => policy.RequireClaim("Permission", "CanDeleteAnyQuestion"));
                  options.AddPolicy("CanEditAnyAnswer", policy => policy.RequireClaim("Permission", "CanEditAnyAnswer"));
                  options.AddPolicy("CanDeleteAnyAnswer", policy => policy.RequireClaim("Permission", "CanDeleteAnyAnswer"));
                  options.AddPolicy("CanDeleteComment", policy => policy.RequireClaim("Permission", "CanDeleteComment"));
                  options.AddPolicy("CanModerateChat", policy => policy.RequireClaim("Permission", "CanModerateChat"));
                  options.AddPolicy("CanViewReports", policy => policy.RequireClaim("Permission", "CanViewReports"));
                  options.AddPolicy("CanWarnUsers", policy => policy.RequireClaim("Permission", "CanWarnUsers"));

                  options.AddPolicy("CanDeleteUser", policy => policy.RequireClaim("Permission", "CanDeleteUser"));
                  options.AddPolicy("CanBanUser", policy => policy.RequireClaim("Permission", "CanBanUser"));
                  options.AddPolicy("CanManageRoles", policy => policy.RequireClaim("Permission", "CanManageRoles"));
                  options.AddPolicy("CanManagePermissions", policy => policy.RequireClaim("Permission", "CanManagePermissions"));
                  options.AddPolicy("CanConfigureSystem", policy => policy.RequireClaim("Permission", "CanConfigureSystem"));
                  options.AddPolicy("CanViewSystemLogs", policy => policy.RequireClaim("Permission", "CanViewSystemLogs"));

                  options.AddPolicy("CanPinQuestion", policy => policy.RequireClaim("Permission", "CanPinQuestion"));
                  options.AddPolicy("CanFeatureQuestion", policy => policy.RequireClaim("Permission", "CanFeatureQuestion"));
                  options.AddPolicy("CanUseAdvancedSearch", policy => policy.RequireClaim("Permission", "CanUseAdvancedSearch"));
                  options.AddPolicy("CanManageTags", policy => policy.RequireClaim("Permission", "CanManageTags"));
                  options.AddPolicy("CanResetUserPassword", policy => policy.RequireClaim("Permission", "CanResetUserPassword"));
                  options.AddPolicy("CanAccessAnalytics", policy => policy.RequireClaim("Permission", "CanAccessAnalytics"));
                  options.AddPolicy("CanSendSystemNotification", policy => policy.RequireClaim("Permission", "CanSendSystemNotification"));

              }
          );*/

            // Identity configuration
            builder.Services.AddIdentity<User, Role>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["secretKey"]!))
                };
            });

            // Dependency Injection for services
            builder.Services.AddScoped<IUserAccountService, UserAccountService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<IUserModerationService, UserModerationService>();
            builder.Services.AddScoped<IQuestionService, QuestionService>();
            builder.Services.AddScoped<IAnswerService, AnswerService>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<IVoteService, VoteService>();
            builder.Services.AddScoped<ITagService, TagService>();
            builder.Services.AddScoped<IBookmarkService, BookmarkService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<RefreshTokenCleaner>();
            builder.Services.AddScoped<UserSettingsService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IMentionService, MentionService>();
            builder.Services.AddScoped<HttpClient>();
            //builder.Services.AddScoped<NotificationHub>();
            builder.Services.AddScoped<CustomExceptionHandlerMiddleware>();
            builder.Services.AddScoped<GeminiService>();
            builder.Services.AddMemoryCache();


            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // this rate limiting built in in .net 6 and above
            // to enable it too on your end point use the attribute [EnableRateLimiting("FixedPolicy")]
            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("FixedPolicy", opt =>
                {
                    opt.Window = TimeSpan.FromMinutes(1);    // Time window of 1 minute
                    opt.PermitLimit = 100;                   // Allow 100 requests per minute
                    opt.QueueLimit = 2;                      // Queue limit of 2
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });
            });
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // Replace the existing AddSerilog code block with the following:
            builder.Host.UseSerilog();
            builder.Services.AddHealthChecks()
                .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), name: "SQL Server");

           
            var app = builder.Build();
            app.UseRateLimiter(); // Enable rate limiting

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        results = report.Entries.Select(e => new {
                            key = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description
                        })
                    });
                    await context.Response.WriteAsync(result);
                }
            });

            app.UseHangfireDashboard();
            RecurringJob.AddOrUpdate<RefreshTokenCleaner>(
             "clean-expired-refresh-tokens",
             cleaner => cleaner.RemoveExpiredTokensAsync(),
             Cron.Hourly);
            app.UseHangfireDashboard("/hangfire");
            app.MapHub<NotificationHub>("/hubs/notification");
            app.UseMiddleware<CustomExceptionHandlerMiddleware>();
            app.UseHttpsRedirection();

            // Enable authentication & authorization
            app.UseAuthentication(); 
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
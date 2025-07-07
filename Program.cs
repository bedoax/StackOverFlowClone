using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackOverFlowClone.Data;
using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Models.Role;
using StackOverFlowClone.Services.Implementations;
using StackOverFlowClone.Services.Interfaces;
using System.Text;
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
            var app = builder.Build();
            app.UseRateLimiter(); // Enable rate limiting

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Enable authentication & authorization
            app.UseAuthentication(); // 🟢 هذا مهم جدًا
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
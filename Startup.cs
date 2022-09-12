using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YgoProgressionDuels.Areas.Identity;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Pages.Account;
using YgoProgressionDuels.Services;

namespace YgoProgressionDuels
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Blazorise
            services
              .AddBlazorise(options =>
              {
                  options.ChangeTextOnKeyPress = true; // optional
              })
              .AddBootstrapProviders()
              .AddFontAwesomeIcons();

            // Use AddDbContextFactory rather than AddDbContext
            // because we want a new DbContext for every action performed
            // rather than one for the whole page's operations
            services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            // This line is required for us to be able to Add-Migration properly with the above AddDbContextFactory
            services.AddScoped(p => p.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

            services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.Lockout.AllowedForNewUsers = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
                .AddRoles<ApplicationRole>() // This line is necessary for Add-Migration if using Roles
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Adding this so we can initialize our database with default admin user and default user in the Configure() method
            //services.AddScoped<IDbInitializer, DbInitializer>();

            // Adding this so we can initialize our booster packs master list in the Configure() method
            services.AddScoped<IWebScraper, WebScraper>();

            // This lets us inject and emailsending service so we can send emails as needed
            //services.Configure<SendGridSettings>(Configuration.GetSection(nameof(SendGridSettings)));
            //services.AddScoped<IEmailSender, SendGridEmailSender>();

            // This lets us upload/delete files/images into the wwwrooot directory
            //services.AddScoped<IFileUploader, FileUploader>();

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
            services.AddDatabaseDeveloperPageExceptionFilter();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, /*IDbInitializer dbInitializer,*/ IWebScraper webScraper)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // These two must be in this exact order
            app.UseAuthentication();
            app.UseAuthorization();

            // Because I decided to make pure Blazor pages for the Identity Account pages,
            // I need to use a middleware for the Login action (it doesn't work in pure Blazor).
            // I'd say it's worth it, to not have both Blazor and Razor Pages to maintain
            app.UseMiddleware<BlazorCookieLoginMiddleware>();

            // Makes sure our master list for our booster packs is initialized and up to date
            //dbInitializer.InitializeAsync().GetAwaiter().GetResult();

            // Checks the web once a day for new booster packs, cards, and banlists
            webScraper.Initialize();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}

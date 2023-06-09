using Ecom_Project_1030.Data;
using Ecom_Project_1030.DataAccess.Repository;
using Ecom_Project_1030.DataAccess.Repository.iRepository;
using Ecom_Project_1030.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecom_Project_1030
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string cs = Configuration.GetConnectionString("conStr"); 
            services.AddDbContext<ApplicationDbContext>
                (options => options.UseSqlServer(cs));

            services.AddDatabaseDeveloperPageExceptionFilter();
            //services.AddScoped<iCategoryRepository, CategoryRepository>();
            //services.AddScoped<iCoverTypeRepository, CoverTypeRepository>();

            services.AddScoped<iUnitofWork, UnitofWork>();
            services.AddScoped<IEmailSender,EmailSender>();
            services.Configure<StripeSettings>(Configuration.GetSection("Stripe"));
            services.Configure<EmailSetting>(Configuration.GetSection("EmailSettings"));


           // services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            // .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders().AddEntityFrameworkStores<ApplicationDbContext>();
           services.AddControllersWithViews();
            services.AddRazorPages();
            services.ConfigureApplicationCookie(option =>
            {
                option.LoginPath = $"/identity/Account/Login";
                option.AccessDeniedPath = $"/identity/Account/Accessdenied";
                option.LogoutPath = $"/identity/Account/Logout";

            });
            services.AddAuthentication().AddFacebook(options =>
            {
                options.AppId = "677177737377108";
                options.AppSecret = "22f6f061341819c90e9d13f346693304";
            });
            services.AddAuthentication().AddGoogle(options =>
             {
                 options.ClientId = "120062753723-nqe5iph63iticnioiab8jprb424vg3a2.apps.googleusercontent.com";
                 options.ClientSecret = "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";
             });
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            StripeConfiguration.ApiKey = Configuration.GetSection("Stripe")["SecretKey"];
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{Area=Customer}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}

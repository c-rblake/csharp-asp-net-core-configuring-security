using ConferenceTracker.Data;
using ConferenceTracker.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.Extensions.Logging;

namespace ConferenceTracker
{
    public class Startup
    {
        private readonly string _allowedOrigins = "_allowedOrigins";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public string SecretMessage { get; set; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("ConferenceTracker"));
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.Configure<CookiePolicyOptions>(options=>
                {
                    options.CheckConsentNeeded = context => true; options.MinimumSameSitePolicy = SameSiteMode.None;
                }); // You'll need to change them based on what cookies you use and what they store
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddTransient<IPresentationRepository, PresentationRepository>();
            services.AddTransient<ISpeakerRepository, SpeakerRepository>();
            services.AddCors(opt=>
                {
                    opt.AddPolicy(_allowedOrigins,
                                    builder => { builder.WithOrigins("http.//pluralsight.com"); });
                    //(This is specifying the name of our CORS policy, and providing what domains will be permitted)
                });
            SecretMessage = Configuration["SecretMessage"];
            //Normally, you'd use this to contain things like connection strings.
            //However, since we're using an InMemory database,
            //this is simply being used as an example, and serves no functional purpose.

        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                logger.LogInformation("Environment is in development");
            }
            else
            { 
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            using (var context = scope.ServiceProvider.GetService<ApplicationDbContext>())
                context.Database.EnsureCreated();

            app.UseCors(_allowedOrigins);

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
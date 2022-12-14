using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using E_Shop.Data;
using E_Shop.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace E_Shop
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
            services.AddControllersWithViews();
            services.AddRazorPages();

            //string database connection
            #region Db Context
            services.AddDbContext<EShopContext>(options =>
            {
                //options.UseSqlServer("Data Source =.; Initial Catalog = EShopDatabse; Integrated Security = true");
                options.UseSqlServer("Server=localhost;Database=EShop;User=sa;Password=Docker@123;");

            });

            #endregion

            #region IOC(Injection Of Control)
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();

            #endregion

            #region Authentication Service
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(
                option =>
                {
                    option.LoginPath = "/Account/Login";
                    option.LogoutPath = "/Account/Logout";
                    option.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                });
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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

            app.UseAuthentication();
            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                
                if (context.Request.Path.StartsWithSegments("/Admin"))
                {
                    if (!context.User.Identity.IsAuthenticated)
                    {
                        context.Response.Redirect("/Account/Login");
                    }
                    else if (!bool.Parse(context.User.FindFirstValue("IsAdmin")))
                    {
                        context.Response.Redirect("/Account/Login");
                    }
                }
                await next.Invoke();
                
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
        
    }
}

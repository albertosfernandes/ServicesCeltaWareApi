using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using ServicesCeltaWare.DAL;
using Microsoft.AspNetCore.HttpOverrides;

namespace ServicesCeltaware.BackEnd
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
            services.AddDbContext<ServicesCeltaWareContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("ServicesCeltaWareConectionString")));

            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));

            services.AddCors(options =>
            {
                options.AddPolicy("BasePolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors("BasePolicy");           

            app.UseHttpsRedirection();
            app.UseMvc(routes =>
            routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}"));

            app.UseMvc();
        }
    }
}


using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ServicesCeltaWare.DAL;

namespace ServicesCeltaware.BackEnd
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "BasePolicy";
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

            services.AddControllers().AddNewtonsoftJson();

            services.AddCors(options =>
            {
                options.AddPolicy("BasePolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
                //.AllowCredentials());
            });

            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            //app.UseAuthorization();
            //app.UseAuthentication();
            app.UseStaticFiles();
            //app.UseMiddleware<AuthenticationMiddleware>();
            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin()
                       //.AllowCredentials()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers()
                .RequireCors(MyAllowSpecificOrigins);
            });
        }
    }
}

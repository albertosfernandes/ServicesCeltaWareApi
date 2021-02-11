
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

//using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServicesCeltaWare.DAL;

using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ServicesCeltaware.ServerAPI
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
            // services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            //services.AddRazorPages();

            //services.AddAuthentication(o => {
            //// This forces challenge results to be handled by Google OpenID Handler, so there's no
            //// need to add an AccountController that emits challenges for Login.
            //o.DefaultChallengeScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
            //// This forces forbid results to be handled by Google OpenID Handler, which checks if
            //// extra scopes are required and does automatic incremental auth.
            //o.DefaultForbidScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
            //// Default scheme that will handle everything else.
            //// Once a user is authenticated, the OAuth2 token info is stored in cookies.
            //o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //})
            //.AddCookie()
            //.AddGoogleOpenIdConnect(options =>
            //    {
            //        options.ClientId = "96332633898-u3qnqp3mtpi04cnvpovrml0fmbiuvi00.apps.googleusercontent.com";
            //        options.ClientSecret = "xQ2eLf_OWneFU2VDqW2aRHfQ";
            //    });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.i)
            //{
            //    app.UseDeveloperExceptionPage();
            //}

            //app.UseForwardedHeaders(new ForwardedHeadersOptions
            //{
            //    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            //});

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


            // app.UseMvc();
        }
    }
}

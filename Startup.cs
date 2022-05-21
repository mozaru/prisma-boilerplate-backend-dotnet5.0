using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using prisma.core;

namespace #NAME_SPACE#
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
            services.AddMvc()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.PropertyNamingPolicy = new Prisma.LowerCaseNamingPolicy());
            services.AddCors();
            services.AddControllers();
            Repositories.RepositoryRegistry.RegistryAll(services);

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            //services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "apicorenet", Version = "v1" });
            });

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = Utils.CriarTokenValidation();
                });
        }

        private string GetDirectoryFrontend()
        {
            string path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"frontend");
            if (System.IO.Directory.Exists(path))
                return path;
            path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(),"..", @"frontend");
            if (System.IO.Directory.Exists(path))
                return path;
            path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "..","..","..","..",@"frontend");
            if (System.IO.Directory.Exists(path))
                return path;
            return System.IO.Directory.GetCurrentDirectory();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "apicorenet v1"));
            }

            //app.UseHttpsRedirection();
            app.UseCors(x => x
                           .AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader());
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<StaticPagesMiddleware>(GetDirectoryFrontend());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore;
using TTForABP.Models;


namespace TTForABP
{
    public class StartUp
    {
        public class Startup
        {
            // Use this method to add services to the container.  
            public void ConfigureServices(IServiceCollection services)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

                services.AddSingleton   <DbManager>();


                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API Name", Version = "v1" });
                });
            }
            // Use this method to configure the HTTP request pipeline.  
            public void Configure(IApplicationBuilder app)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API Name V1");
                });
            }
        }
        
    }
}

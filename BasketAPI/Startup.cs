using Business.Queries.BasketQuery;
using Core.Config;
using Data.Entities;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BasketAPI
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
            var assemblies = GetAssemblies().ToArray();

            services.AddControllers().AddFluentValidation(x => x.RegisterValidatorsFromAssemblies(assemblies));
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BasketAPI", Version = "v1" });
            });
            services.AddMediatR(assemblies);
            var config = Configuration.GetSection("ConnectionString").Get<DbConfig>();
            services.AddSingleton(config);
            services.AddScoped(typeof(Entities), typeof(Entities));

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {   
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BasketAPI v1");
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            yield return typeof(CreateBasketQuery).GetTypeInfo().Assembly;
            yield return typeof(Startup).GetTypeInfo().Assembly;
        }
    }
}

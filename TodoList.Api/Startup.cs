using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using TodoList.Business.Interfaces;
using TodoList.Business.Services;
using TodoList.Data.Context;
using TodoList.Data.Interfaces;
using TodoList.Data.Repositories;

namespace TodoList.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllHeaders",
                      builder =>
                      {
                          builder.AllowAnyOrigin()
                                 .AllowAnyHeader()
                                 .AllowAnyMethod();
                      });
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TodoList.Api", Version = "v1" });
            });

            /* 
             * Here, we are using In Memory provider but for other environments connection string can come from environment variables 
             * or from App Configuration service offered by Azure as we can then store it in key vault due to its sensitive nature 
             * 
             * As communication with an API is a disconnected scenario UseQueryTrackingBehavior with NoTracking 
             * will ensure EF is not tracking queried entities resulting in a better performance
            */
            services.AddDbContext<TodoContext>(options => 
                            options.UseInMemoryDatabase(databaseName: "TodoDb")
                                   .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            services.AddScoped<ITodoItemService, TodoItemService>();
            services.AddScoped<ITodoItemRepository, TodoItemRepository>();

            // This code will automatically register all Auto Mapper profiles rather than doing them individually
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.EnableFilter();
                    options.EnableValidator();
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoList.Api v1");
                });
            }

            if (!env.IsDevelopment())            
                app.UseHttpsRedirection();            

            app.UseRouting();

            app.UseCors("AllowAllHeaders");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

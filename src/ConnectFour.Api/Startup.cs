using ConnectFour.Api.Filters;
using ConnectFour.DataLayer.Repositories.GameRepository;
using ConnectFour.ServiceLayer.GameService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Text.Json.Serialization;

namespace ConnectFour.Api
{
    /// <summary>
    /// The Startup class configures services and the app's request pipeline.
    /// </summary>
    public class Startup
    {
        private readonly IWebHostEnvironment _hostingEnv;

        /// <summary>
        /// Not for use in <see cref="ConfigureServices(IServiceCollection)"/>!
        /// ASP.NET Core 3.x doesn't allow that: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.1#create-logs
        /// Writing logs before completion of the DI container setup in the Startup.ConfigureServices method is not supported:
        ///         Logger injection into the Startup constructor is not supported.
        ///         Logger injection into the Startup.ConfigureServices method signature is not supported
        /// </summary>
        private ILogger<Startup> _logger;

        private IConfiguration Configuration { get; }

        /// <summary>
        /// Constructor. 
        /// Application startup order: 
        /// 1) <see cref="Startup(IWebHostEnvironment, IConfiguration)"/>
        /// 2) <see cref="ConfigureServices(IServiceCollection)"/>
        /// 3) <see cref="Configure(IApplicationBuilder, IWebHostEnvironment, ILogger{Startup})"/>
        /// </summary>
        /// <param name="env">Provides information about the web hosting environment an application is running in.</param>
        /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            _hostingEnv = env;
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// <param name="services">Specifies the contract for a collection of service descriptors.</param>
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services
                // Add and configure the logging provider before anything else
                .AddLogging(builder =>
                {
                    builder
                        .AddConfiguration(Configuration.GetSection("Logging"))
                        .AddConsole()
                        .AddDebug();
                });

            services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                });

            services
                .AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("1.0.0", new OpenApiInfo
                    {
                        Version = "1.0.0",
                        Title = "98Point6 Drop-Token",
                        Description = "98Point6 Drop-Token (ASP.NET Core 3.1)",
                        Contact = new OpenApiContact()
                        {
                            Name = "Aaron Burke",
                            Url = new Uri("https://aaron-burke.me"),
                            Email = "aaron@focuszonedevelopment.com"
                        }
                    });
                    c.EnableAnnotations();
                    c.IncludeXmlComments($"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}{_hostingEnv.ApplicationName}.xml");

                    // Include DataAnnotation attributes on Controller Action parameters as Swagger validation rules (e.g required, pattern, ..)
                    // Use [ValidateModelState] on Actions to actually validate it in C# as well!
                    c.OperationFilter<GeneratePathParamsValidationFilter>();
                });

            // Configure dependency injection
            // TODO: When real DB backing store is added, these can be transient
            services.AddSingleton<IGameRepository, GameRepository>();
            services.AddSingleton<IGameService, GameService>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="logger"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            _logger = logger;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app
                .UseSwagger()
                .UseSwaggerUI(c =>
                {
                    // use the SwaggerGen generated Swagger contract (generated from C# classes)
                    c.SwaggerEndpoint("/swagger/1.0.0/swagger.json", "98Point6 Drop-Token");
                });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

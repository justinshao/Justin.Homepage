using Justin.Homepage.Repositories;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Justin.Homepage
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddTransient<IArticleRepository, LocalArticleRepository>();
            services.AddInstance<IConfiguration>(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseExceptionHandler("/error/500");
            app.UseStatusCodePagesWithRedirects("/error/{0}");
            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "article",
                    template: "article/{id}",
                    defaults: new { controller = "Article", action = "Index" });

                routes.MapRoute(
                    name: "new-article",
                    template: "article-n",
                    defaults: new { controller = "Article", action = "New" });

                routes.MapRoute(
                    name: "edit-article",
                    template: "article-e/{id?}",
                    defaults: new { controller = "Article", action = "Edit" });

                routes.MapRoute(
                    name: "delete-article",
                    template: "article-d/{id}",
                    defaults: new { controller = "Article", action = "Delete" });

                routes.MapRoute(
                    name: "article-image",
                    template: "article-image/{articleId}/{id}",
                    defaults: new { controller = "Article", action = "Image" });

                routes.MapRoute(
                    name: "error",
                    template: "error/{code?}",
                    defaults: new { controller = "Base", action = "StatusCode" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}

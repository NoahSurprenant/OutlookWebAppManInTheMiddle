
using HttpContextMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace OutlookWebAppManInTheMiddle
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.RegisterDefaultReverseProxy();
            builder.Services.AddScoped<IContextMapper, OutlookWebAppContextMapper>();

            builder.Services.AddDbContextFactory<OutlookWebAppDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("Database");

                var sqliteBuilder = new SqliteConnectionStringBuilder(connectionString);

                sqliteBuilder.DataSource = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sqliteBuilder.DataSource));

                connectionString = sqliteBuilder.ToString();

                options.UseSqlite(connectionString);
            });

            var app = builder.Build();

            var factory = app.Services.GetRequiredService<IDbContextFactory<OutlookWebAppDbContext>>();
            var context = factory.CreateDbContext();
            context.Database.Migrate();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ExceptionLoggerMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthorization();

            app.MapFallbackToContextMapper();
            app.MapControllers();

            app.Run();
        }
    }
}
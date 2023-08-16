
using HttpContextMapper;
using HttpContextMapper.Extensions;
using HttpContextMapper.Middlewares;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

namespace OutlookWebAppManInTheMiddle
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting application");

                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog();

                // Add services to the container.

                builder.Services.AddControllers();
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                builder.Services.AddScoped<IContextMapper, OutlookWebAppContextMapper>();

                builder.Services.AddDbContextFactory<OutlookWebAppDbContext>(options =>
                {
                    var connectionString = builder.Configuration.GetConnectionString("Database");

                    var sqliteBuilder = new SqliteConnectionStringBuilder(connectionString);

                    sqliteBuilder.DataSource = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sqliteBuilder.DataSource));

                    connectionString = sqliteBuilder.ToString();

                    Log.Logger.Information("ConnectionString: {ConnectionString}", connectionString);

                    options.UseSqlite(connectionString);
                });

                builder.ConfigureAndRegisterDefaultHttpClientWithForwardProxy();

                var app = builder.Build();

                var options = app.Services.GetService<IOptions<ForwardProxyOptions>>();
                Log.Logger.Information("Proxy Config {Host}:{Port}", options?.Value.Host, options?.Value.Port);

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
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }

            
        }
    }
}
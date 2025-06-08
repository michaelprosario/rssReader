using AppInfra.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogReaderApp.Data
{
    /// <summary>
    /// Extension methods for database initialization
    /// </summary>
    public static class DbInitializerExtension
    {
        /// <summary>
        /// Ensures the database is created and migrated
        /// </summary>
        /// <param name="app">The WebApplication instance</param>
        public static void CreateAndSeedDatabase(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    
                    // Ensure the database is created
                    context.Database.EnsureCreated();
                    
                    // Initialize with sample data if needed
                    DbInitializer.Initialize(context).Wait();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while creating/seeding the database.");
                }
            }
        }
    }
}

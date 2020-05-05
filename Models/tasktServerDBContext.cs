using Microsoft.EntityFrameworkCore;

namespace tasktServer.Models
{
    public class TasktDatabaseContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder contextBuilder)
        {
            if (!contextBuilder.IsConfigured)
            {

                var connection = DatabaseConfiguration.ConnectionString;
                contextBuilder.UseSqlServer(connection);
            }
        }
        public TasktDatabaseContext(DbContextOptions<TasktDatabaseContext> options)
            : base(options)
        { }
        public TasktDatabaseContext()
        { }

        public DbSet<Task> Tasks { get; set; }
        public DbSet<WorkerPool> WorkerPools { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<PublishedScript> PublishedScripts { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<BotStoreModel> BotStore { get; set; }
        public DbSet<UserProfile> LoginProfiles { get; set; }
    }

    public static class DatabaseConfiguration
    {
        public static string ConnectionString { get; set; }
    }



}

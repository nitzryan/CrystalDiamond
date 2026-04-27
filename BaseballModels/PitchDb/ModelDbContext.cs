using Microsoft.EntityFrameworkCore;

namespace ModelDb
{
	public class ModelDbContext : DbContext
	{
		public DbSet<Output_PitchValue> Output_PitchValue {get; set;}
		public DbSet<Models_PitchValue> Models_PitchValue {get; set;}
		public DbSet<ModelTrainingHistory_PitchValue> ModelTrainingHistory_PitchValue {get; set;}
		public DbSet<Output_PitchValueAggregation> Output_PitchValueAggregation {get; set;}

		public ModelDbContext(DbContextOptions<ModelDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Output_PitchValue>().HasKey(f => new {f.Model,f.GameId,f.PitchId,f.ModelRun});
			modelBuilder.Entity<Models_PitchValue>().HasKey(f => new {f.Id});
			modelBuilder.Entity<ModelTrainingHistory_PitchValue>().HasKey(f => new {f.ModelId,f.ModelRun});
			modelBuilder.Entity<Output_PitchValueAggregation>().HasKey(f => new {f.Model,f.GameId,f.PitchId});
		}
	}
}
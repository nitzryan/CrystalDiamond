using Microsoft.EntityFrameworkCore;

namespace PitchDb
{
	public class PitchDbContext : DbContext
	{
		public DbSet<Models_PitchValue> Models_PitchValue {get; set;}
		public DbSet<ModelTrainingHistory_PitchValue> ModelTrainingHistory_PitchValue {get; set;}
		public DbSet<PlayersInTrainingData> PlayersInTrainingData {get; set;}
		public DbSet<YearLeagueDeviations> YearLeagueDeviations {get; set;}
		public DbSet<PitcherStuff> PitcherStuff {get; set;}
		public DbSet<Output_PitchValue> Output_PitchValue {get; set;}
		public DbSet<Output_PitchValueAggregation> Output_PitchValueAggregation {get; set;}

		public PitchDbContext(DbContextOptions<PitchDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Models_PitchValue>().HasKey(f => new {f.Id});
			modelBuilder.Entity<ModelTrainingHistory_PitchValue>().HasKey(f => new {f.ModelId,f.ModelRun});
			modelBuilder.Entity<PlayersInTrainingData>().HasKey(f => new {f.MlbId,f.ModelId,f.ModelRun});
			modelBuilder.Entity<YearLeagueDeviations>().HasKey(f => new {f.ModelId,f.Year,f.Balls,f.Strikes});
			modelBuilder.Entity<PitcherStuff>().HasKey(f => new {f.MlbId,f.Year,f.Month,f.Model,f.GameId,f.PitchType,f.Scenario});
			modelBuilder.Entity<Output_PitchValue>().HasKey(f => new {f.Model,f.GameId,f.PitchId,f.ModelRun});
			modelBuilder.Entity<Output_PitchValueAggregation>().HasKey(f => new {f.Model,f.GameId,f.PitchId});
		}
	}
}
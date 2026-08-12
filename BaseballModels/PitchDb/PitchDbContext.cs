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
		public DbSet<PitchValue> PitchValue {get; set;}
		public DbSet<PitchModelResultBasis> PitchModelResultBasis {get; set;}
		public DbSet<PitcherStatcastMonth> PitcherStatcastMonth {get; set;}

		public PitchDbContext(DbContextOptions<PitchDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Models_PitchValue>().HasKey(f => new {f.Id});
			modelBuilder.Entity<ModelTrainingHistory_PitchValue>().HasKey(f => new {f.ModelId,f.Year,f.ModelRun});
			modelBuilder.Entity<PlayersInTrainingData>().HasKey(f => new {f.MlbId,f.ModelId,f.Year,f.ModelRun});
			modelBuilder.Entity<YearLeagueDeviations>().HasKey(f => new {f.ModelId,f.Year,f.Balls,f.Strikes});
			modelBuilder.Entity<PitcherStuff>().HasKey(f => new {f.MlbId,f.Year,f.Month,f.Model,f.GameId,f.PitchType,f.Scenario});
			modelBuilder.Entity<Output_PitchValue>().HasKey(f => new {f.Model,f.GameId,f.PitchId,f.ModelYear,f.ModelRun});
			modelBuilder.Entity<Output_PitchValueAggregation>().HasKey(f => new {f.Model,f.GameId,f.PitchId,f.ModelYear});
			modelBuilder.Entity<PitchValue>().HasKey(f => new {f.ModelId,f.GameId,f.PitchId});
			modelBuilder.Entity<PitchModelResultBasis>().HasKey(f => new {f.Year,f.ModelId,f.CountBalls,f.CountStrikes,f.OutputType});
			modelBuilder.Entity<PitcherStatcastMonth>().HasKey(f => new {f.MlbId,f.Year,f.Month,f.ModelId});
		}
	}
}
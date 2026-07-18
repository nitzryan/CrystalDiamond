using Microsoft.EntityFrameworkCore;

namespace ModelDb
{
	public class ModelDbContext : DbContext
	{
		public DbSet<Output_PlayerWar> Output_PlayerWar {get; set;}
		public DbSet<Output_PlayerHighestLevel> Output_PlayerHighestLevel {get; set;}
		public DbSet<Output_HitterStats> Output_HitterStats {get; set;}
		public DbSet<Output_PitcherStats> Output_PitcherStats {get; set;}
		public DbSet<Output_College_Hitter> Output_College_Hitter {get; set;}
		public DbSet<Output_College_Pitcher> Output_College_Pitcher {get; set;}
		public DbSet<Model_TrainingHistory> Model_TrainingHistory {get; set;}
		public DbSet<PlayersInTrainingData> PlayersInTrainingData {get; set;}
		public DbSet<ModelId> ModelId {get; set;}
		public DbSet<Output_HitterStatsAggregation> Output_HitterStatsAggregation {get; set;}
		public DbSet<Output_PitcherStatsAggregation> Output_PitcherStatsAggregation {get; set;}
		public DbSet<Output_PlayerWarAggregation> Output_PlayerWarAggregation {get; set;}
		public DbSet<Output_PlayerHighestLevelAggregation> Output_PlayerHighestLevelAggregation {get; set;}
		public DbSet<Output_College_HitterAggregation> Output_College_HitterAggregation {get; set;}
		public DbSet<Output_College_PitcherAggregation> Output_College_PitcherAggregation {get; set;}

		public ModelDbContext(DbContextOptions<ModelDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Output_PlayerWar>().HasKey(f => new {f.MlbId,f.ModelId,f.IsHitter,f.ModelRun,f.Year,f.Month});
			modelBuilder.Entity<Output_PlayerHighestLevel>().HasKey(f => new {f.MlbId,f.ModelId,f.IsHitter,f.ModelRun,f.Year,f.Month});
			modelBuilder.Entity<Output_HitterStats>().HasKey(f => new {f.MlbId,f.ModelId,f.ModelRun,f.Year,f.Month,f.LevelId});
			modelBuilder.Entity<Output_PitcherStats>().HasKey(f => new {f.MlbId,f.ModelId,f.ModelRun,f.Year,f.Month,f.LevelId});
			modelBuilder.Entity<Output_College_Hitter>().HasKey(f => new {f.TbcId,f.ModelId,f.ModelRun,f.Year});
			modelBuilder.Entity<Output_College_Pitcher>().HasKey(f => new {f.TbcId,f.ModelId,f.ModelRun,f.Year});
			modelBuilder.Entity<Model_TrainingHistory>().HasKey(f => new {f.ModelName,f.IsHitter,f.ModelRun});
			modelBuilder.Entity<PlayersInTrainingData>().HasKey(f => new {f.MlbId,f.TbcId,f.ModelId,f.ModelRun,f.IsHitter});
			modelBuilder.Entity<ModelId>().HasKey(f => new {f.Id});
			modelBuilder.Entity<Output_HitterStatsAggregation>().HasKey(f => new {f.MlbId,f.ModelId,f.Year,f.Month,f.LevelId});
			modelBuilder.Entity<Output_PitcherStatsAggregation>().HasKey(f => new {f.MlbId,f.ModelId,f.Year,f.Month,f.LevelId});
			modelBuilder.Entity<Output_PlayerWarAggregation>().HasKey(f => new {f.MlbId,f.ModelId,f.IsHitter,f.Year,f.Month});
			modelBuilder.Entity<Output_PlayerHighestLevelAggregation>().HasKey(f => new {f.MlbId,f.ModelId,f.IsHitter,f.Year,f.Month});
			modelBuilder.Entity<Output_College_HitterAggregation>().HasKey(f => new {f.TbcId,f.ModelId,f.Year});
			modelBuilder.Entity<Output_College_PitcherAggregation>().HasKey(f => new {f.TbcId,f.ModelId,f.Year});
		}
	}
}
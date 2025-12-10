using Microsoft.EntityFrameworkCore;

namespace SiteDb
{
	public class SiteDbContext : DbContext
	{
		public DbSet<Player> Player {get; set;}
		public DbSet<HitterYearStats> HitterYearStats {get; set;}
		public DbSet<HitterMonthStats> HitterMonthStats {get; set;}
		public DbSet<PitcherYearStats> PitcherYearStats {get; set;}
		public DbSet<PitcherMonthStats> PitcherMonthStats {get; set;}
		public DbSet<PlayerModel> PlayerModel {get; set;}
		public DbSet<PlayerRank> PlayerRank {get; set;}
		public DbSet<HitterWarRank> HitterWarRank {get; set;}
		public DbSet<PitcherWarRank> PitcherWarRank {get; set;}
		public DbSet<TeamRank> TeamRank {get; set;}
		public DbSet<Models> Models {get; set;}
		public DbSet<PlayerYearPositions> PlayerYearPositions {get; set;}
		public DbSet<HomeData> HomeData {get; set;}
		public DbSet<HomeDataType> HomeDataType {get; set;}

		public SiteDbContext(DbContextOptions<SiteDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Player>().HasKey(f => new {f.MlbId});
			modelBuilder.Entity<HitterYearStats>().HasKey(f => new {f.MlbId,f.Year,f.TeamId,f.LeagueId});
			modelBuilder.Entity<HitterMonthStats>().HasKey(f => new {f.MlbId,f.Year,f.Month,f.TeamId,f.LeagueId});
			modelBuilder.Entity<PitcherYearStats>().HasKey(f => new {f.MlbId,f.Year,f.TeamId,f.LeagueId});
			modelBuilder.Entity<PitcherMonthStats>().HasKey(f => new {f.MlbId,f.Year,f.Month,f.TeamId,f.LeagueId});
			modelBuilder.Entity<PlayerModel>().HasKey(f => new {f.MlbId,f.Year,f.Month,f.ModelId,f.IsHitter});
			modelBuilder.Entity<PlayerRank>().HasKey(f => new {f.MlbId,f.ModelId,f.IsHitter,f.Year,f.Month});
			modelBuilder.Entity<HitterWarRank>().HasKey(f => new {f.MlbId,f.ModelId,f.Year,f.Month});
			modelBuilder.Entity<PitcherWarRank>().HasKey(f => new {f.MlbId,f.ModelId,f.Year,f.Month});
			modelBuilder.Entity<TeamRank>().HasKey(f => new {f.TeamId,f.ModelId,f.Year,f.Month});
			modelBuilder.Entity<Models>().HasKey(f => new {f.ModelId});
			modelBuilder.Entity<PlayerYearPositions>().HasKey(f => new {f.MlbId,f.Year,f.IsHitter});
			modelBuilder.Entity<HomeData>().HasKey(f => new {f.Year,f.Month,f.RankType,f.ModelId,f.IsWar,f.Rank});
			modelBuilder.Entity<HomeDataType>().HasKey(f => new {f.Type});
		}
	}
}
using Microsoft.EntityFrameworkCore;

namespace SiteDb
{
	public class SiteDbContext : DbContext
	{
		public DbSet<Player> Player {get; set;}
		public DbSet<HitterStats> HitterStats {get; set;}
		public DbSet<PitcherStats> PitcherStats {get; set;}
		public DbSet<PlayerModel> PlayerModel {get; set;}
		public DbSet<PlayerRank> PlayerRank {get; set;}

		public SiteDbContext(DbContextOptions<SiteDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Player>().HasKey(f => new {f.MlbId});
			modelBuilder.Entity<HitterStats>().HasKey(f => new {f.MlbId,f.Year,f.TeamId,f.LeagueId});
			modelBuilder.Entity<PitcherStats>().HasKey(f => new {f.MlbId,f.Year,f.TeamId,f.LeagueId});
			modelBuilder.Entity<PlayerModel>().HasKey(f => new {f.MlbId,f.Year,f.Month,f.ModelName});
			modelBuilder.Entity<PlayerRank>().HasKey(f => new {f.MlbId,f.ModelName,f.Year,f.Month});
		}
	}
}
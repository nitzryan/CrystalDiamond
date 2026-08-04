using Microsoft.EntityFrameworkCore;

namespace PitchTrackingDb
{
	public class PitchTrackingDbContext : DbContext
	{
		public DbSet<PitchData> PitchData {get; set;}
		public DbSet<PitchFlightpath> PitchFlightpath {get; set;}
		public DbSet<PitchFlightpathGameDelta> PitchFlightpathGameDelta {get; set;}

		public PitchTrackingDbContext(DbContextOptions<PitchTrackingDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<PitchData>().HasKey(f => new {f.GameId,f.PitchId});
			modelBuilder.Entity<PitchFlightpath>().HasKey(f => new {f.GameId,f.PitchId});
			modelBuilder.Entity<PitchFlightpathGameDelta>().HasKey(f => new {f.GameId,f.PitchId});
		}
	}
}
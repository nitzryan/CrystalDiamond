using Microsoft.EntityFrameworkCore;

namespace PitchTrackingDb
{
	public class PitchTrackingDbContext : DbContext
	{
		public DbSet<PitchData> PitchData {get; set;}
		public DbSet<PitchFlightpath> PitchFlightpath {get; set;}
		public DbSet<PitchFlightpathGameDelta> PitchFlightpathGameDelta {get; set;}
		public DbSet<PitchZoneCompetitive> PitchZoneCompetitive {get; set;}
		public DbSet<PitchZoneNonCompetitive> PitchZoneNonCompetitive {get; set;}

		public PitchTrackingDbContext(DbContextOptions<PitchTrackingDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<PitchData>().HasKey(f => new {f.GameId,f.PitchId});
			modelBuilder.Entity<PitchFlightpath>().HasKey(f => new {f.GameId,f.PitchId});
			modelBuilder.Entity<PitchFlightpathGameDelta>().HasKey(f => new {f.GameId,f.PitchId,f.FastballPitchType});
			modelBuilder.Entity<PitchZoneCompetitive>().HasKey(f => new {f.Year,f.PitchClass,f.PitIsR,f.HitIsR,f.CountBalls,f.CountStrikes,f.ZoneRow,f.ZoneCol});
			modelBuilder.Entity<PitchZoneNonCompetitive>().HasKey(f => new {f.Year,f.PitchClass,f.PitIsR,f.HitIsR,f.CountBalls,f.CountStrikes});
		}
	}
}
using Microsoft.EntityFrameworkCore;

namespace SwingDecisionsDb
{
	public class SwingDecisionsDbContext : DbContext
	{
		public DbSet<SwingDecision> SwingDecision {get; set;}
		public DbSet<SwingResultAggregation> SwingResultAggregation {get; set;}

		public SwingDecisionsDbContext(DbContextOptions<SwingDecisionsDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<SwingDecision>().HasKey(f => new {f.GameId,f.PitchId});
			modelBuilder.Entity<SwingResultAggregation>().HasKey(f => new {f.HitterId,f.PitcherId,f.LevelId,f.Year,f.Month,f.PitchGroup});
		}
	}
}
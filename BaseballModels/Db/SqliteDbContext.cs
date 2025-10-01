using Microsoft.EntityFrameworkCore;

namespace Db
{
	public class SqliteDbContext : DbContext
	{
		public DbSet<Draft_Results> Draft_Results {get; set;}
		public DbSet<League_Factors> League_Factors {get; set;}
		public DbSet<Level_Factors> Level_Factors {get; set;}
		public DbSet<Level_HitterStats> Level_HitterStats {get; set;}
		public DbSet<Level_PitcherStats> Level_PitcherStats {get; set;}
		public DbSet<Model_HitterStats> Model_HitterStats {get; set;}
		public DbSet<Model_PitcherStats> Model_PitcherStats {get; set;}
		public DbSet<Model_HitterValue> Model_HitterValue {get; set;}
		public DbSet<Model_PitcherValue> Model_PitcherValue {get; set;}
		public DbSet<Model_PlayerWar> Model_PlayerWar {get; set;}
		public DbSet<Model_Players> Model_Players {get; set;}
		public DbSet<Model_TrainingHistory> Model_TrainingHistory {get; set;}
		public DbSet<Output_PlayerWar> Output_PlayerWar {get; set;}
		public DbSet<Output_HitterStats> Output_HitterStats {get; set;}
		public DbSet<Output_PitcherStats> Output_PitcherStats {get; set;}
		public DbSet<Output_PlayerWarAggregation> Output_PlayerWarAggregation {get; set;}
		public DbSet<Output_HitterStatsAggregation> Output_HitterStatsAggregation {get; set;}
		public DbSet<Output_PitcherStatsAggregation> Output_PitcherStatsAggregation {get; set;}
		public DbSet<Park_Factors> Park_Factors {get; set;}
		public DbSet<Park_ScoringData> Park_ScoringData {get; set;}
		public DbSet<Player> Player {get; set;}
		public DbSet<Player_CareerStatus> Player_CareerStatus {get; set;}
		public DbSet<Player_Hitter_GameLog> Player_Hitter_GameLog {get; set;}
		public DbSet<Player_Hitter_MonthAdvanced> Player_Hitter_MonthAdvanced {get; set;}
		public DbSet<Player_Hitter_MonthStats> Player_Hitter_MonthStats {get; set;}
		public DbSet<Player_Hitter_MonthlyRatios> Player_Hitter_MonthlyRatios {get; set;}
		public DbSet<Player_Hitter_YearAdvanced> Player_Hitter_YearAdvanced {get; set;}
		public DbSet<Player_OrgMap> Player_OrgMap {get; set;}
		public DbSet<Transaction_Log> Transaction_Log {get; set;}
		public DbSet<Player_Pitcher_GameLog> Player_Pitcher_GameLog {get; set;}
		public DbSet<Player_Pitcher_MonthAdvanced> Player_Pitcher_MonthAdvanced {get; set;}
		public DbSet<Player_Pitcher_MonthStats> Player_Pitcher_MonthStats {get; set;}
		public DbSet<Player_Pitcher_MonthlyRatios> Player_Pitcher_MonthlyRatios {get; set;}
		public DbSet<Player_Pitcher_YearAdvanced> Player_Pitcher_YearAdvanced {get; set;}
		public DbSet<Player_ServiceLapse> Player_ServiceLapse {get; set;}
		public DbSet<Player_ServiceTime> Player_ServiceTime {get; set;}
		public DbSet<Player_YearlyWar> Player_YearlyWar {get; set;}
		public DbSet<Player_MonthlyWar> Player_MonthlyWar {get; set;}
		public DbSet<Pre05_Players> Pre05_Players {get; set;}
		public DbSet<Team_League_Map> Team_League_Map {get; set;}
		public DbSet<Team_OrganizationMap> Team_OrganizationMap {get; set;}
		public DbSet<Team_Parents> Team_Parents {get; set;}
		public DbSet<Leagues> Leagues {get; set;}
		public DbSet<Ranking_Prospect> Ranking_Prospect {get; set;}
		public DbSet<Site_PlayerBio> Site_PlayerBio {get; set;}
		public DbSet<ModelIdx> ModelIdx {get; set;}
		public DbSet<PlayersInTrainingData> PlayersInTrainingData {get; set;}

		public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Draft_Results>().HasKey(f => new {f.Year,f.Pick});
			modelBuilder.Entity<League_Factors>().HasKey(f => new {f.LeagueId,f.Year});
			modelBuilder.Entity<Level_Factors>().HasKey(f => new {f.LevelId,f.Year});
			modelBuilder.Entity<Level_HitterStats>().HasKey(f => new {f.LevelId,f.Year,f.Month});
			modelBuilder.Entity<Level_PitcherStats>().HasKey(f => new {f.LevelId,f.Year,f.Month});
			modelBuilder.Entity<Model_HitterStats>().HasKey(f => new {f.MlbId,f.Year,f.Month});
			modelBuilder.Entity<Model_PitcherStats>().HasKey(f => new {f.MlbId,f.Year,f.Month});
			modelBuilder.Entity<Model_HitterValue>().HasKey(f => new {f.MlbId,f.Year,f.Month});
			modelBuilder.Entity<Model_PitcherValue>().HasKey(f => new {f.MlbId,f.Year,f.Month});
			modelBuilder.Entity<Model_PlayerWar>().HasKey(f => new {f.MlbId,f.Year,f.IsHitter});
			modelBuilder.Entity<Model_Players>().HasKey(f => new {f.MlbId});
			modelBuilder.Entity<Model_TrainingHistory>().HasKey(f => new {f.ModelName,f.IsHitter,f.ModelIdx});
			modelBuilder.Entity<Output_PlayerWar>().HasKey(f => new {f.MlbId,f.Model,f.IsHitter,f.ModelIdx,f.Year,f.Month});
			modelBuilder.Entity<Output_HitterStats>().HasKey(f => new {f.MlbId,f.Model,f.LevelId,f.ModelIdx});
			modelBuilder.Entity<Output_PitcherStats>().HasKey(f => new {f.MlbId,f.Model,f.LevelId,f.ModelIdx});
			modelBuilder.Entity<Output_PlayerWarAggregation>().HasKey(f => new {f.MlbId,f.Model,f.IsHitter,f.Year,f.Month});
			modelBuilder.Entity<Output_HitterStatsAggregation>().HasKey(f => new {f.MlbId,f.Model,f.LevelId});
			modelBuilder.Entity<Output_PitcherStatsAggregation>().HasKey(f => new {f.MlbId,f.Model,f.LevelId});
			modelBuilder.Entity<Park_Factors>().HasKey(f => new {f.TeamId,f.Year});
			modelBuilder.Entity<Park_ScoringData>().HasKey(f => new {f.TeamId,f.Year,f.LeagueId});
			modelBuilder.Entity<Player>().HasKey(f => new {f.MlbId});
			modelBuilder.Entity<Player_CareerStatus>().HasKey(f => new {f.MlbId});
			modelBuilder.Entity<Player_Hitter_GameLog>().HasKey(f => new {f.GameLogId});
			modelBuilder.Entity<Player_Hitter_MonthAdvanced>().HasKey(f => new {f.MlbId,f.LevelId,f.Year,f.Month,f.TeamId,f.LeagueId});
			modelBuilder.Entity<Player_Hitter_MonthStats>().HasKey(f => new {f.MlbId,f.Year,f.Month,f.LevelId});
			modelBuilder.Entity<Player_Hitter_MonthlyRatios>().HasKey(f => new {f.MlbId,f.Year,f.Month,f.LevelId});
			modelBuilder.Entity<Player_Hitter_YearAdvanced>().HasKey(f => new {f.MlbId,f.LevelId,f.Year,f.TeamId,f.LeagueId});
			modelBuilder.Entity<Player_OrgMap>().HasKey(f => new {f.MlbId,f.Year,f.Month,f.Day});
			modelBuilder.Entity<Transaction_Log>().HasKey(f => new {f.TransactionId});
			modelBuilder.Entity<Player_Pitcher_GameLog>().HasKey(f => new {f.GameLogId});
			modelBuilder.Entity<Player_Pitcher_MonthAdvanced>().HasKey(f => new {f.MlbId,f.LevelId,f.Year,f.Month,f.TeamId,f.LeagueId});
			modelBuilder.Entity<Player_Pitcher_MonthStats>().HasKey(f => new {f.MlbId,f.Year,f.Month,f.LevelId});
			modelBuilder.Entity<Player_Pitcher_MonthlyRatios>().HasKey(f => new {f.MlbId,f.Year,f.Month,f.LevelId});
			modelBuilder.Entity<Player_Pitcher_YearAdvanced>().HasKey(f => new {f.MlbId,f.LevelId,f.Year,f.TeamId,f.LeagueId});
			modelBuilder.Entity<Player_ServiceLapse>().HasKey(f => new {f.MlbId});
			modelBuilder.Entity<Player_ServiceTime>().HasKey(f => new {f.MlbId,f.Year});
			modelBuilder.Entity<Player_YearlyWar>().HasKey(f => new {f.MlbId,f.Year,f.IsHitter});
			modelBuilder.Entity<Player_MonthlyWar>().HasKey(f => new {f.MlbId,f.Year,f.Month,f.IsHitter});
			modelBuilder.Entity<Pre05_Players>().HasKey(f => new {f.MlbId});
			modelBuilder.Entity<Team_League_Map>().HasKey(f => new {f.TeamId,f.LeagueId,f.Year});
			modelBuilder.Entity<Team_OrganizationMap>().HasKey(f => new {f.TeamId,f.Year});
			modelBuilder.Entity<Team_Parents>().HasKey(f => new {f.Id});
			modelBuilder.Entity<Leagues>().HasKey(f => new {f.Id});
			modelBuilder.Entity<Ranking_Prospect>().HasKey(f => new {f.MlbId,f.Year,f.Month,f.ModelIdx,f.IsHitter});
			modelBuilder.Entity<Site_PlayerBio>().HasKey(f => new {f.Id});
			modelBuilder.Entity<ModelIdx>().HasKey(f => new {f.Id});
			modelBuilder.Entity<PlayersInTrainingData>().HasKey(f => new {f.MlbId,f.ModelIdx});
		}
	}
}
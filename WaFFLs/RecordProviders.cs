using RazorEngine.Templating;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using WaFFLs.Generation;
using WaFFLs.Generation.Models;

namespace WaFFLs
{

    [Title(Text = "Most Fantasy Bowl Appearances")]
    [Summary(Text = "Discover the teams that have reached the Fantasy Bowl more than any other. These franchises have consistently battled their way to the championship game, proving their staying power at the top!")]
    public class MostFantasyBowlAppearances : ICareerRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public MostFantasyBowlAppearances(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<CareerRecord> GetData()
        {
            Dictionary<Team, List<int>> cache = new Dictionary<Team, List<int>>();

            foreach (var season in _leagueData.Seasons)
            {
                var bowl = season.Playoffs.Single(w => w.Name.StartsWith("Fantasy Bowl")).Games.SingleOrDefault();
                if (bowl == null)
                    break;

                Team champion = bowl.GetWinningTeam();
                Team runnerup = bowl.GetLosingTeam();

                bool exists = cache.TryGetValue(champion, out List<int> years);
                if (!exists)
                {
                    years = new List<int>();
                    cache.Add(champion, years);
                }
                years.Add(season.Year);


                exists = cache.TryGetValue(runnerup, out years);
                if (!exists)
                {
                    years = new List<int>();
                    cache.Add(runnerup, years);
                }
                years.Add(season.Year);
            }

            return cache.OrderByDescending(t => t.Value.Count)
                        .ThenByDescending(t => t.Value.Max())
                        .Take(_count)
                        .Select(t => new CareerRecord() { Value = t.Value.Count.WithCommas(), Team = t.Key, Notes = string.Join(",", t.Value) })
                        .ToList();
        }
    }

    [Title(Text = "Most Fantasy Bowl Championships")]
    [Summary(Text = "See which teams have turned championship dreams into reality! These dynasties have conquered the Fantasy Bowl time and time again, building a legacy of winning when it matters most.")]
    public class MostFantasyBowlChampionships : ICareerRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public MostFantasyBowlChampionships(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<CareerRecord> GetData()
        {
            Dictionary<Team, List<int>> cache = new Dictionary<Team, List<int>>();

            foreach (var season in _leagueData.Seasons)
            {
                var bowl = season.Playoffs.Single(w => w.Name.StartsWith("Fantasy Bowl")).Games.SingleOrDefault();
                if (bowl == null)
                    break;

                Team champion = bowl.GetWinningTeam();

                bool exists = cache.TryGetValue(champion, out List<int> years);
                if (!exists)
                {
                    years = new List<int>();
                    cache.Add(champion, years);
                }
                years.Add(season.Year);
            }

            return cache.OrderByDescending(t => t.Value.Count)
                        .ThenByDescending(t => t.Value.Max())
                        .Take(_count)
                        .Select(t => new CareerRecord() { Value = t.Value.Count.WithCommas(), Team = t.Key, Notes = string.Join(",", t.Value) })
                        .ToList();
        }
    }

    [Title(Text = "Most Career Wins")]
    [Summary(Text = "Which teams have racked up the most victories across both regular seasons and playoffs? Witness the winningest franchises in league history and their relentless pursuit of victory!")]
    public class MostWinsInCareerIncludingPlayoffs : ICareerRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public MostWinsInCareerIncludingPlayoffs(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<CareerRecord> GetData()
        {
            var teams = _leagueData.Teams.Select(t => new { Team = t, Wins = t.Games.Count(g => g.IsWinningTeam(t)) })
                                         .OrderByDescending(x => x.Wins)
                                         .Take(_count);

            return teams.Select(t => new CareerRecord() { Value = t.Wins.WithCommas(), Team = t.Team }).ToList();
        }
    }

    [Title(Text = "Most Career Playoff Wins")]
    [Summary(Text = "Playoff pressure? These teams thrive on it. See who has dominated the postseason and built a legacy of clutch performances when the stakes are highest!")]
    public class MostCareerPlayoffWins : ICareerRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public MostCareerPlayoffWins(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<CareerRecord> GetData()
        {
            var teams = _leagueData.Teams.Select(t => new { Team = t, Wins = t.Games.Where(g => !g.Week.Name.StartsWith("Week ")).Count(g => g.IsWinningTeam(t)) })
                                         .OrderByDescending(x => x.Wins)
                                         .Take(_count);

            return teams.Select(t => new CareerRecord() { Value = t.Wins.WithCommas(), Team = t.Team }).ToList();
        }
    }

    [Title(Text = "Most Career Losses")]
    [Summary(Text = "Every loss tells a story. Explore the teams that have faced adversity more than any other, and see how they’ve endured through the toughest seasons!")]
    public class MostLossesInCareerIncludingPlayoffs : ICareerRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public MostLossesInCareerIncludingPlayoffs(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<CareerRecord> GetData()
        {
            var teams = _leagueData.Teams.Select(t => new { Team = t, Wins = t.Games.Count(g => g.IsLosingTeam(t)) })
                                        .OrderByDescending(x => x.Wins)
                                        .Take(_count);

            return teams.Select(t => new CareerRecord() { Value = t.Wins.WithCommas(), Team = t.Team }).ToList();
        }
    }

    [Title(Text = "Most Wins in a Single Season")]
    [Summary(Text = "Which teams have put together the most dominant single-season runs? Relive the campaigns that set new standards for excellence—including playoff victories!")]
    public class MostWinsInASeason : ISeasonRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public MostWinsInASeason(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<SeasonRecord> GetData()
        {
            var teams = _leagueData.Teams.SelectMany(t => t.Games.GroupBy(g => g.Week.Season.Year)
                                                                 .Select(s => new { Team = t, Year = s.Key, Wins = s.Count(g => g.IsWinningTeam(t)) }))
                                                                 .OrderByDescending(x => x.Wins)
                                                                 .Take(_count);

            return teams.Select(t => new SeasonRecord() { Value = t.Wins.ToString(), Team = t.Team, Year = t.Year }).ToList();
        }
    }

    [Title(Text = "Most Points Scored in a Season")]
    [Summary(Text = "Offensive explosions! Find out which teams have posted the highest point totals in a single season and redefined what’s possible on the scoreboard!")]
    public class MostPointsScoredInASeason : ISeasonRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public MostPointsScoredInASeason(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<SeasonRecord> GetData()
        {
            var teams = _leagueData.Teams.SelectMany(t => t.Games.Where(g => g.Week.IsRegular())
                                                           .GroupBy(g => g.Week.Season.Year)
                                                           .Select(s => new { Team = t, Year = s.Key, PointsScored = s.Sum(g => g.GetTeamScore(t)) }))
                                         .OrderByDescending(x => x.PointsScored)
                                         .Take(_count);

            return teams.Select(t => new SeasonRecord() { Value = t.PointsScored.WithCommas(), Team = t.Team, Year = t.Year }).ToList();
        }
    }

    [Title(Text = "Most Points Scored in a Career")]
    [Summary(Text = "Consistency is king. These teams have piled up points year after year, cementing their place among the league’s all-time greats!")]
    public class MostPointsScoredInCareer : ICareerRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public MostPointsScoredInCareer(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<CareerRecord> GetData()
        {
            var teams = _leagueData.Teams.Select(t => new { Team = t, PointsScored = t.Games.Sum(g => g.GetTeamScore(t)) })
                                        .OrderByDescending(x => x.PointsScored)
                                        .Take(_count);

            return teams.Select(t => new CareerRecord() { Value = t.PointsScored.WithCommas(), Team = t.Team }).ToList();
        }
    }

    [Title(Text = "Most 1000-Point Games - Career")]
    [Summary(Text = "Breaking the 1000-point barrier is a mark of greatness. See which teams have done it most often and set the league on fire!")]
    public class Most1000PointGames : ICareerRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public Most1000PointGames(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<CareerRecord> GetData()
        {
            var scores = _leagueData.Seasons.SelectMany(s => s.WeeksIncludingPlayoffs())
                               .SelectMany(w => w.Games)
                               .SelectMany(g => new[] { g.Home, g.Away })
                               .Where(s => s.Score >= 1000);

            var groupedScores = scores.GroupBy(g => g.Team).OrderByDescending(x => x.Count())
                               .Take(_count);

            return groupedScores.Select(t => new CareerRecord() { Value = t.Count().WithCommas(), Team = t.Key }).ToList();
        }
    }

    [Title(Text = "Most 1000 Point Games - Season + Playoffs")]
    [Summary(Text = "Who’s had the hottest hand in a single season, including the playoffs? These teams have delivered jaw-dropping performances when it mattered most!")]
    public class Most1000PointGamesInASeasonAndPlayoffs : ISeasonRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public Most1000PointGamesInASeasonAndPlayoffs(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<SeasonRecord> GetData()
        {
            var scores = _leagueData.Seasons.SelectMany(s => s.WeeksIncludingPlayoffs())
                               .SelectMany(w => w.Games)
                               .SelectMany(g => new[] { g.Home, g.Away })
                               .Where(s => s.Score >= 1000);

            var groupedScores = scores.GroupBy(g => new { g.Game.Week.Season.Year, g.Team }).OrderByDescending(x => x.Count())
                                      .Take(_count);

            return groupedScores.Select(t => new SeasonRecord() { Value = t.Count().ToString(), Team = t.Key.Team, Year = t.Key.Year}).ToList();
        }
    }

    [Title(Text = "Most 1000 Point Games - Season")]
    [Summary(Text = "Regular season fireworks! Discover the teams that have stacked up 1000-point games in a single campaign and electrified fans week after week!")]
    public class Most1000PointGamesInASeason : ISeasonRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public Most1000PointGamesInASeason(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<SeasonRecord> GetData()
        {
            var scores = _leagueData.Seasons.SelectMany(s => s.Weeks)
                               .SelectMany(w => w.Games)
                               .SelectMany(g => new[] { g.Home, g.Away })
                               .Where(s => s.Score >= 1000);

            var groupedScores = scores.GroupBy(g => new { g.Game.Week.Season.Year, g.Team }).OrderByDescending(x => x.Count())
                                      .Take(_count);

            return groupedScores.Select(t => new SeasonRecord() { Value = t.Count().ToString(), Team = t.Key.Team, Year = t.Key.Year }).ToList();
        }
    }

    [Title(Text = "Best Score - Single Game")]
    [Summary(Text = "Relive the most unforgettable games where teams put up monster numbers and shattered records in a single outing!")]
    public class BestSingleGameScores : IIndividualGameRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public BestSingleGameScores(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<IndividualGameRecord> GetData()
        {
            var scores = _leagueData.Seasons.SelectMany(s => s.Weeks)
                                           .SelectMany(w => w.Games)
                                           .SelectMany(g => new[] { g.Home, g.Away });

            var orderedScores = scores.OrderByDescending(s => s.Score).Take(_count);

            return orderedScores.Select(t =>
                new IndividualGameRecord()
                {
                    Value = t.Score,
                    Team = t.Team,
                    Game = t.Game.Week,
                }).ToList();
        }
    }

    [Title(Text = "Lowest Score - Single Game")]
    [Summary(Text = "Not every game is a shootout. These are the lowest scoring performances, where points were hard to come by!")]
    public class WorstSingleGameScores : IIndividualGameRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public WorstSingleGameScores(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<IndividualGameRecord> GetData()
        {
            var scores = _leagueData.Seasons.SelectMany(s => s.WeeksIncludingPlayoffs())
                                           .SelectMany(w => w.Games)
                                           .SelectMany(g => new[] { g.Home, g.Away });

            var orderedScores = scores.OrderBy(s => s.Score).Take(_count);

            return orderedScores.Select(t =>
                new IndividualGameRecord()
                {
                    Value = t.Score,
                    Team = t.Team,
                    Game = t.Game.Week,
                }).ToList();
        }
    }

    [Title(Text = "Best Score - in a Loss")]
    [Summary(Text = "Scoring big but coming up short—these teams put up huge numbers in defeat, proving that sometimes even greatness isn’t enough!")]
    public class BestSingleGameLosingScores : IIndividualGameRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public BestSingleGameLosingScores(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<IndividualGameRecord> GetData()
        {
            var scores = _leagueData.Seasons.SelectMany(s => s.Weeks)
                                           .SelectMany(w => w.Games)
                                           .SelectMany(g => new[] { g.Home, g.Away });

            var orderedScores = scores.OrderBy(s => s.Score, IntegerComparer.Descending).Where(s => s.Game.GetLosingScore() == s).Take(_count);

            return orderedScores.Select(t =>
                new IndividualGameRecord()
                {
                    Value = t.Score,
                    Team = t.Team,
                    Game = t.Game.Week,
                }).ToList();
        }
    }

    [Title(Text = "Best Combined Scores - Single Game")]
    [Summary(Text = "When both teams light up the scoreboard, history is made. Check out the games with the most combined points ever!")]
    public class HighestScoringGames : IGameRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public HighestScoringGames(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<GameRecord> GetData()
        {

            var orderedGames = _leagueData.Seasons.SelectMany(s => s.Weeks)
                                           .SelectMany(w => w.Games)
                                           .OrderByDescending(g => g.Home.Score + g.Away.Score)
                                           .Take(_count);

            return orderedGames.Select(t =>
                new GameRecord()
                {
                    Value = $"{t.Home.Score}-{t.Away.Score}",
                    Team1 = t.Home.Team,
                    Team2 = t.Away.Team,
                    Game = t.Week,
                }).ToList();
        }
    }

    [Title(Text = "Closest Scores - Single Game")]
    [Summary(Text = "Every point counts! These nail-biters were decided by the slimmest of margins, keeping fans on the edge of their seats until the final whistle!")]
    public class ClosestGames : IGameRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public ClosestGames(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<GameRecord> GetData()
        {

            var orderedGames = _leagueData.Seasons.SelectMany(s => s.WeeksIncludingPlayoffs())
                                          .SelectMany(w => w.Games)
                                          .OrderBy(g => Math.Abs(g.Home.Score - g.Away.Score))
                                          .ThenByDescending(g => g.Week.Season.Year)
                                          .Take(_count);

            return orderedGames.Select(t =>
                new GameRecord()
                {
                    Value = $"{t.Home.Score}-{t.Away.Score}",
                    Team1 = t.Home.Team,
                    Team2 = t.Away.Team,
                    Game = t.Week,
                }).ToList();
        }
    }

    [Title(Text = "Longest Winning Streaks")]
    [Summary(Text = "Unstoppable! These teams strung together the longest winning streaks, proving their dominance week after week!")]
    public class LongestWinningStreaks : IStreakRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public LongestWinningStreaks(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<StreakRecord> GetData()
        {
            var allStreaks = new List<List<Game>>();
            foreach (var team in _leagueData.Teams)
            {
                var t = team;
                var orderedGames = team.Games.GroupBy(g => g.Week.Season).OrderBy(g => g.Key.Year).SelectMany(g => g).ToList();
                var teamStreaks = StreakHelper.GetAllStreaks(orderedGames, g => g.IsWinningTeam(t)).ToList();
                allStreaks.AddRange(teamStreaks);
            }

            var topStreaks = allStreaks.OrderByDescending(s => s.Count)
                                       .ThenByDescending(s => s.Last().Week.Season.Year)
                                       .Take(_count);

            return topStreaks.Select(s => new StreakRecord()
            {
                Value = s.Count,
                Team = s.First().GetWinningTeam(),
                From = s.First().Week,
                To = s.Last().Week,
            }).ToList();
        }
    }

    [Title(Text = "Longest Losing Streaks")]
    [Summary(Text = "Endurance through adversity—these are the longest losing streaks, where teams battled through tough times and kept fighting!")]
    public class LongestLosingStreaks : IStreakRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public LongestLosingStreaks(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<StreakRecord> GetData()
        {
            var allStreaks = new List<List<Game>>();
            foreach (var team in _leagueData.Teams)
            {
                var t = team;
                var orderedGames = team.Games.GroupBy(g => g.Week.Season).OrderBy(g => g.Key.Year).SelectMany(g => g).ToList();

                var teamStreaks = StreakHelper.GetAllStreaks(orderedGames, g => g.IsLosingTeam(t)).ToList();
                allStreaks.AddRange(teamStreaks);
            }

            var topStreaks = allStreaks.OrderByDescending(s => s.Count)
                                       .ThenByDescending(s => s.Last().Week.Season.Year)
                                       .Take(_count);

            return topStreaks.Select(s => new StreakRecord()
            {
                Value = s.Count,
                Team = s.First().GetLosingTeam(),
                From = s.First().Week,
                To = s.Last().Week,
            }).ToList();
        }
    }

    [Title(Text = "Longest 1000-Point Streaks")]
    [Summary(Text = "Sustained excellence! These teams have delivered 1000-point games in consecutive weeks, setting a new standard for offensive firepower!")]
    public class Longest1000PointStreaks : IStreakRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public Longest1000PointStreaks(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<StreakRecord> GetData()
        {
            var allStreaks = new List<List<Game>>();
            foreach (var team in _leagueData.Teams)
            {
                var t = team;
                var orderedGames = team.Games.GroupBy(g => g.Week.Season).OrderBy(g => g.Key.Year).SelectMany(g => g).ToList();
                var teamStreaks = StreakHelper.GetAllStreaks(orderedGames, g => g.GetTeamScore(t) >= 1000).ToList();
                allStreaks.AddRange(teamStreaks);
            }

            List<StreakRecord> results = new List<StreakRecord>();

            var topStreaks = allStreaks.OrderByDescending(s => s.Count).Take(_count);
            foreach (var streak in topStreaks)
            {
                var team = streak.First().Home.Team;
                if (!streak.TrueForAll(g => g.Away.Team == team || g.Home.Team == team))
                {
                    team = streak.First().Away.Team;
                    if (!streak.TrueForAll(g => g.Away.Team == team || g.Home.Team == team))
                    {
                        throw new Exception();
                    }
                }

                var r = new StreakRecord()
                {
                    Value = streak.Count,
                    Team = team,
                    From = streak.First().Week,
                    To = streak.Last().Week,
                };
                results.Add(r);
            }

            return results;
        }
    }

    [Title(Text = "Best Head-to-Head Records")]
    [Summary(Text = "Rivalries are the heart of competition. See which teams have absolutely owned their matchups and built legendary head-to-head records!")]
    public class CareerHeadToHeadRecords : ICareerRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public CareerHeadToHeadRecords(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<CareerRecord> GetData()
        {
            List<HeadToHeadRecords> results = new List<HeadToHeadRecords>();

            var orderedteams = _leagueData.Teams.OrderBy(t => t.Name);
            foreach (Team team in orderedteams)
            {
                Team teamData = team;

                var grouping = teamData.Games.GroupBy(g => g.OpponentOf(teamData)).OrderByDescending(g => g.Count());
                foreach (var group in grouping)
                {
                    int wins = group.Where(g => g.Week.Name.StartsWith("Week ")).Count(g => g.GetWinningTeam() == teamData);
                    int losses = group.Where(g => g.Week.Name.StartsWith("Week ")).Count(g => g.GetLosingTeam() == teamData);

                    var r = new HeadToHeadRecords()
                    {
                        Team = teamData,
                        Opponent = group.Key,
                        Wins = wins,
                        Losses = losses
                    };

                    results.Add(r);
                }
            }

            return results.Where(r => r.Wins + r.Losses > 10)
                          .OrderByDescending(r => r.Percentage)
                          .Take(_count)
                          .Select(r => new CareerRecord() {
                              Value = string.Format("{0} - {1}  {2}", r.Wins, r.Losses, r.Percentage.AsWinningPercentage()),
                              Team = r.Team,
                              Notes = string.Format("vs {0}", r.Opponent.Name) })
                          .ToList();
        }

        private class HeadToHeadRecords
        {
            public Team Team { get; set; }

            public Team Opponent { get; set; }

            public int Wins { get; set; }

            public int Losses { get; set; }

            public double Percentage => (double)Wins / (double)(Wins + Losses);
        }
    }

    [Title(Text = "Best Season Record - Excluding Playoffs")]
    [Summary(Text = "Which teams have put together the most dominant single-season runs? Relive the campaigns that set new standards for excellence—regular season only!")]
    public class BestRecordInASeason : ISeasonRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public BestRecordInASeason(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<SeasonRecord> GetData()
        {
            var teams = _leagueData.Teams.SelectMany(t => t.Games.Where(g => g.Week.IsRegular())
                                                                 .GroupBy(g => g.Week.Season.Year)
                                                                 .Select(s => new { Team = t, Year = s.Key, Wins = s.Count(g => g.IsWinningTeam(t)), Losses = s.Count(g => g.IsLosingTeam(t)) }))
                                                                 .OrderByDescending(s => GetWinPercentage(s.Wins, s.Losses))
                                                                 .ThenByDescending(s => s.Year)
                                                                 .Take(_count);

            return teams.Select(t => new SeasonRecord() { Value = string.Format("{0} - {1} {2:F3}", t.Wins, t.Losses, GetWinPercentage(t.Wins, t.Losses)), Team = t.Team, Year = t.Year }).ToList();
        }

        public double GetWinPercentage(int wins, int losses)
        {
            return (double)wins / (double)(wins + losses);
        }   
    }

    [Title(Text = "Most 10 Win Seasons")]
    [Summary(Text = "Which teams have been consistently great season after season? These teams have won 10 games or more in a single season a record number of times!")]
    public class Most10WinSeasons : ICareerRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public Most10WinSeasons(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<CareerRecord> GetData()
        {
            var teams = _leagueData.Teams.SelectMany(t => t.Games.GroupBy(g => g.Week.Season.Year)
                                                                 .Select(s => new { Team = t, Year = s.Key, Wins = s.Count(g => g.Week.IsRegular() && g.IsWinningTeam(t)) })
                                                                 .Where(s => s.Wins >= 10)
                                                                 .GroupBy(s => s.Team)
                                                                 .Select(s => new { Team = t, Seasons = s.Count(), LastYear = s.Max(x => x.Year) }));

            return teams.OrderByDescending(t => t.Seasons)
                        .ThenByDescending(t => t.LastYear)
                        .Take(_count)
                        .Select(t => new CareerRecord() { Value = t.Seasons.ToString(), Team = t.Team }).ToList();
        }
    }

    [Title(Text = "Most 10 Loss Seasons")]
    [Summary(Text = "Which teams have struggled the most season after season? These teams have lost 10 games or more in a single season more than any other!")]
    public class Most10LossSeasons : ICareerRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public Most10LossSeasons(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<CareerRecord> GetData()
        {
            var teams = _leagueData.Teams.SelectMany(t => t.Games.GroupBy(g => g.Week.Season.Year)
                                                                 .Select(s => new { Team = t, Year = s.Key, Losses = s.Count(g => g.Week.IsRegular() && g.IsLosingTeam(t)) })
                                                                 .Where(s => s.Losses >= 10)
                                                                 .GroupBy(s => s.Team)
                                                                 .Select(s => new { Team = t, Seasons = s.Count(), LastYear = s.Max(x => x.Year) }));

            return teams.OrderByDescending(t => t.Seasons)
                        .ThenByDescending(t => t.LastYear)
                        .Take(_count)
                        .Select(t => new CareerRecord() { Value = t.Seasons.ToString(), Team = t.Team }).ToList();
        }
    }

    [Title(Text = "Most Seasons Played")]
    [Summary(Text = "Who are the elder statesmen? These teams have been around the longest, showing unmatched longevity in the league!")]
    public class TotalSeasonsPlayed : ICareerRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public TotalSeasonsPlayed(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<CareerRecord> GetData()
        {
            var teams = _leagueData.Teams.SelectMany(t => t.Games.GroupBy(g => g.Week.Season.Year)
                                                                 .Select(s => new { Team = t, Year = s.Key })
                                                                 .GroupBy(g => g.Team)
                                                                 .Select(s => new { Team = s.Key, SeasonsPlayed = s.Count(), LastYear = s.Max(x => x.Year) }));

            return teams.OrderByDescending(t => t.SeasonsPlayed)
                        .ThenByDescending(t => t.LastYear)
                        .Take(_count)
                        .Select(t => new CareerRecord() { Value = t.SeasonsPlayed.ToString(), Team = t.Team }).ToList();
        }
    }

    [Title(Text = "Most Playoff Appearances")]
    [Summary(Text = "These teams have proven their consistency and resilience by reaching the playoffs more times than any other. Explore the franchises that are always in the hunt for a championship, year after year!")]
    public class MostPlayOffAppearances : ICareerRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public MostPlayOffAppearances(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<CareerRecord> GetData()
        {
            var teams = _leagueData.Teams.SelectMany(t => t.Games.GroupBy(g => g.Week.Season.Year)
                                                                 .Where(s => s.Any(g => g.Week.IsPlayoff()))
                                                                 .Select(s => new { Team = t, Year = s.Key })
                                                                 .GroupBy(g => g.Team)
                                                                 .Select(s => new { Team = s.Key, Playoffs = s.Count(), LastYear = s.Max(x => x.Year) }));

            return teams.OrderByDescending(t => t.Playoffs)
                        .ThenByDescending(t => t.LastYear)
                        .Take(_count)
                        .Select(t => new CareerRecord() { Value = t.Playoffs.ToString(), Team = t.Team }).ToList();
        }
    }

    public class StreakHelper
    {
        public static List<List<TSource>> GetAllStreaks<TSource>(List<TSource> source, Func<TSource, bool> predicate)
        {
            var streaks = new List<List<TSource>>();

            List<TSource> current = null;
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    if (current == null)
                    {
                        current = new List<TSource>();
                    }

                    current.Add(item);
                }
                else
                {
                    if (current != null && current.Count > 1)
                    {
                        streaks.Add(current);
                    }

                    current = null;
                }
            }

            if (current != null && current.Count > 1)
            {
                streaks.Add(current);
            }

            return streaks;
        }
    }
}

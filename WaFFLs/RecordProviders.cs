using System;
using System.Collections.Generic;
using System.Linq;
using WaFFLs.Generation;
using WaFFLs.Generation.Models;

namespace WaFFLs
{

    public class GetMostPointsScoredInASeason : ISeasonRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public GetMostPointsScoredInASeason(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<SeasonRecord> GetData()
        {
            var teams = _leagueData.Teams.SelectMany(t => t.Games.Where(g => g.Week.Name.StartsWith("Week "))
                                                           .GroupBy(g => g.Week.Season.Year)
                                                           .Select(s => new { Team = t, Year = s.Key, PointsScored = s.Sum(g => g.GetTeamScore(t)) }))
                                         .OrderByDescending(x => x.PointsScored)
                                         .Take(_count);

            return teams.Select(t => new SeasonRecord() { Value = t.PointsScored, Team = t.Team, Year = t.Year }).ToList();
        }
    }

    public class GetMostPointsScoredAllTime : ICareerRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public GetMostPointsScoredAllTime(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<CareerRecord> GetData()
        {
            var teams = _leagueData.Teams.Select(t => new { Team = t, PointsScored = t.Games.Sum(g => g.GetTeamScore(t)) })
                                        .OrderByDescending(x => x.PointsScored)
                                        .Take(_count);

            return teams.Select(t => new CareerRecord() { Value = t.PointsScored, Team = t.Team }).ToList();
        }
    }

    public class GetTeamsWithMost1000PointGames : ICareerRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public GetTeamsWithMost1000PointGames(League leagueData, int count = 10)
        {
            _leagueData = leagueData;
            _count = count;
        }

        public List<CareerRecord> GetData()
        {
            var scores = _leagueData.Seasons.SelectMany(s => s.Weeks)
                               .SelectMany(w => w.Games)
                               .SelectMany(g => new[] { g.Home, g.Away })
                               .Where(s => s.Score >= 1000);

            var groupedScores = scores.GroupBy(g => g.Team).OrderByDescending(x => x.Count())
                               .Take(_count);

            return groupedScores.Select(t => new CareerRecord() { Value = t.Count(), Team = t.Key }).ToList();
        }
    }

    public class GetTopAllTimeScores : IIndividualGameRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public GetTopAllTimeScores(League leagueData, int count = 10)
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

    public class GetTopLosingScores : IIndividualGameRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public GetTopLosingScores(League leagueData, int count = 10)
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

    public class GetHighestScoringGames : IGameRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public GetHighestScoringGames(League leagueData, int count = 10)
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
                    Value = $"{t.Home.Score,4}-{t.Away.Score,-4}",
                    Team1 = t.Home.Team,
                    Team2 = t.Away.Team,
                    Game = t.Week,
                }).ToList();
        }
    }

    public class GetLongestWinningStreaks : IStreakRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public GetLongestWinningStreaks(League leagueData, int count = 10)
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
                var orderedGames = team.Games.GroupBy(g => g.Week.Season).OrderBy(g => g.Key.Year).SelectMany(g => g).Where(g => g.Week.Name.StartsWith("Week ")).ToList();
                var teamStreaks = StreakHelper.GetAllStreaks(orderedGames, g => g.IsWinningTeam(t)).ToList();
                allStreaks.AddRange(teamStreaks);
            }

            var topStreaks = allStreaks.OrderByDescending(s => s.Count).Take(_count);

            return topStreaks.Select(s => new StreakRecord()
            {
                Value = s.Count,
                Team = s.First().GetWinningTeam(),
                From = s.First().Week,
                To = s.Last().Week,
            }).ToList();
        }
    }

    public class GetLongestLosingStreaks : IStreakRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public GetLongestLosingStreaks(League leagueData, int count = 10)
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
                var orderedGames = team.Games.GroupBy(g => g.Week.Season).OrderBy(g => g.Key.Year).SelectMany(g => g).Where(g => g.Week.Name.StartsWith("Week ")).ToList();

                var teamStreaks = StreakHelper.GetAllStreaks(orderedGames, g => g.IsLosingTeam(t)).ToList();
                allStreaks.AddRange(teamStreaks);
            }

            var topStreaks = allStreaks.OrderByDescending(s => s.Count).Take(_count);

            return topStreaks.Select(s => new StreakRecord()
            {
                Value = s.Count,
                Team = s.First().GetLosingTeam(),
                From = s.First().Week,
                To = s.Last().Week,
            }).ToList();
        }
    }

    public class GetLongest1000PointStreaks : IStreakRecordProvider
    {
        private readonly League _leagueData;
        private readonly int _count;

        public GetLongest1000PointStreaks(League leagueData, int count = 10)
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
                var orderedGames = team.Games.GroupBy(g => g.Week.Season).OrderBy(g => g.Key.Year).SelectMany(g => g).Where(g => g.Week.Name.StartsWith("Week ")).ToList();
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

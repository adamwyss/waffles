using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WaFFLs
{
    class Program
    {
        private static void UpdateYear(int year)
        {
            var remote = new OnlineWaFFLDataSource();
            var local = new CachedWaFFLDataSource();
            var txt = remote.GetStandingsDataForYear(year);
            local.Cache(year, txt);
        }

        static void Main(string[] args)
        {
            //UpdateYear(2015);


            League leagueData = new League();

            IWaFFLDataSource dataProvider = new CachedWaFFLDataSource();
            ITeamResolver teamResolver = new LeagueTeamResolver(leagueData);
            var parser = new HtmlPageParser(dataProvider, teamResolver);
            parser.Parse(leagueData);

//            GetAllTeamsEverInLeague(leagueData);

//            GetAllTeamsInSeason(leagueData, 2016);

            var team2 = leagueData.Teams.Single(t => t.Name == "Washington Whompers");
            GetAverages(team2);


            var team = leagueData.Teams.Single(t => t.Name == "Just Too Lucky");
           // GetAllTimeRecordFor(team);
           // GetHeadToHeadRecordsFor(team);
           // ListAllGamesBySeasonForTeam(team);
            GetAverages(team);


//            GetTopAllTimeScores(leagueData);
//            GetTopLosingScores(leagueData);
//            GetTeamsWithMost1000PointGames(leagueData);
//            GetHighestScoringGames(leagueData);





//            GetTeamsAndYearsPlayed(leagueData);
//            GetTeamsAndPlayoffAppearances(leagueData);
//            GetChampionships(leagueData);

            GetAverages(leagueData);



            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }

        }

        private static void ListAllGamesBySeasonForTeam(Team team)
        {
            Console.WriteLine("{0}", team.Name);

            var seasonGroups = team.Games.GroupBy(g => g.Week.Season);
            foreach (var games in seasonGroups)
            {
                var regularGames = games.Where(g => g.Week.Name.StartsWith("Week ")).ToList();

                int wins = regularGames.Count(g => g.GetWinningTeam() == team);
                int losses = regularGames.Count(g => g.GetLosingTeam() == team);
                double winPercentage = (double)wins / (double)(wins + losses);
                winPercentage *= 100;
                bool valid = (wins + losses == 15 && games.Key.Year == 1996) || (wins + losses == 14 && games.Key.Year != 1996);
                var winloss = string.Format("{0}-{1}{2}", wins, losses, valid ? "" : "*");

                Console.WriteLine(" {0} - {1,5} {2,10:00.0}%", games.Key.Year, winloss, winPercentage);

                foreach (var game in games)
                {
                    TeamScore myScore, theirScore;

                    if (game.Home.Team == team)
                    {
                        myScore = game.Home;
                        theirScore = game.Away;
                    }
                    else if (game.Away.Team == team)
                    {
                        myScore = game.Away;
                        theirScore = game.Home;
                    }
                    else throw new Exception();

                    bool win = myScore.Score > theirScore.Score;

                    Console.WriteLine("  {0} {1,4}-{2,4} vs {3,-30} {4}", win ? "W" : "L", myScore.Score, theirScore.Score, theirScore.Team.Name, game.Week.Name);
                }
            }
        }

        private static void GetAllTeamsEverInLeague(League leagueData)
        {
            var orderedteams = leagueData.Teams.OrderBy(t => t.Name);

            foreach (Team team in orderedteams)
            {
                Console.WriteLine("{0}", team.Name);

                var seasons = team.Games.Select(g => g.Week.Season).Distinct();
                foreach (Season season in seasons)
                {
                    Team closureTeam = team;
                    
                    var games = season.Weeks.SelectMany(w => w.Games).Where(g => g.Home.Team == closureTeam || g.Away.Team == closureTeam);
                    int wins = games.Count(g => g.GetWinningTeam() == team);
                    int losses = games.Count(g => g.GetLosingTeam() == team);

                    double winPercentage = (double)wins / (double)(wins + losses);
                    winPercentage *= 100;

                    bool valid = (wins + losses == 15 && season.Year == 1996) || (wins + losses == 14 && season.Year != 1996);

                    var winloss = string.Format("{0}-{1}{2}", wins, losses, valid ? "" : "*");
                    Console.WriteLine("  {0}:  {1,5} {2,10:00.0}%", season.Year, winloss, winPercentage);
                }
            }
        }

        private static void GetAllTeamsInSeason(League leagueData, int year)
        {
            Console.WriteLine("{0}", year);

            var orderedteams = leagueData.Teams.OrderBy(t => t.Name);
            foreach (Team team in orderedteams)
            {
                var seasons = team.Games.Select(g => g.Week.Season).Distinct();
                var targetseason = seasons.SingleOrDefault(s => s.Year == year);
                if (targetseason != null)
                {
                    Team closureTeam = team;

                    var games =
                        targetseason.Weeks.SelectMany(w => w.Games).Where(g => g.Home.Team == closureTeam || g.Away.Team == closureTeam);
                    int wins = games.Count(g => g.GetWinningTeam() == team);
                    int losses = games.Count(g => g.GetLosingTeam() == team);

                    double winPercentage = (double)wins / (double)(wins + losses);
                    winPercentage *= 100;

                    bool valid = (wins + losses == 15 && targetseason.Year == 1996) || (wins + losses == 14 && targetseason.Year != 1996);

                    var winloss = string.Format("{0}-{1}{2}", wins, losses, valid ? "" : "*");
                    Console.WriteLine("  {0, -30}: {1,5} {2,10:00.0}%", team.Name, winloss, winPercentage);
                }
            }
        }

        private static void Get2008RecordForFightingCalrissians(League leagueData)
        {
            var team = leagueData.Teams.Single(t => t.Name == "Fighting Calrissians");
            var games = leagueData.Seasons.Single(s => s.Year == 2008)
                .Weeks.SelectMany(w => w.Games)
                .Where(g => g.Home.Team == team || g.Away.Team == team);

            int wins = games.Count(g => g.GetWinningTeam() == team);
            int losses = games.Count(g => g.GetLosingTeam() == team);

            var scores = games.SelectMany(s => new[] { s.Home, s.Away }).Where(s => s.Team == team);
            int allPointsFor2008 = scores.Sum(s => s.Score);
        }

        private static void GetAllTimeRecordFor(Team team)
        {
            {
                int wins = team.Games.Where(g => g.Week.Name.StartsWith("Week ")).Count(g => g.GetWinningTeam() == team);
                int losses = team.Games.Where(g => g.Week.Name.StartsWith("Week ")).Count(g => g.GetLosingTeam() == team);

                double winPercentage = (double)wins / (double)(wins + losses);
                winPercentage *= 100.0;

                Console.WriteLine("{0,3}-{1,3}  {2,4:0.0}%", wins, losses, winPercentage);
            }

            {
                int wins = team.Games.Where(g => !g.Week.Name.StartsWith("Week ")).Count(g => g.GetWinningTeam() == team);
                int losses = team.Games.Where(g => !g.Week.Name.StartsWith("Week ")).Count(g => g.GetLosingTeam() == team);

                double winPercentage = (double)wins / (double)(wins + losses);
                winPercentage *= 100.0;

                Console.WriteLine("{0,3}-{1,3}  {2,4:0.0}%", wins, losses, winPercentage);
            }
        }

        private static void GetTopAllTimeScores(League leagueData, int count = 10)
        {
            var scores = leagueData.Seasons.SelectMany(s => s.Weeks)
                                           .SelectMany(w => w.Games)
                                           .SelectMany(g => new[] { g.Home, g.Away });

            var orderedScores = scores.OrderByDescending(s => s.Score).Take(count);

            int index = 1;
            foreach (var score in orderedScores)
            {
                Console.WriteLine("{4,2}. {0,-4} {1,-30} {2}, {3}", score.Score, score.Team.Name, score.Game.Week.Name, score.Game.Week.Season.Year, index++);
            }
        }

        private static void GetTopLosingScores(League leagueData, int count = 10)
        {
            var scores = leagueData.Seasons.SelectMany(s => s.Weeks)
                                           .SelectMany(w => w.Games)
                                           .SelectMany(g => new[] { g.Home, g.Away });

            var orderedScores = scores.OrderBy(s => s.Score, IntegerComparer.Descending).Where(s => s.Game.GetLosingScore() == s).Take(count);

            int index = 1;
            foreach (var score in orderedScores)
            {
                Console.WriteLine("{4,2}. {0,-4} {1,-30} {2}, {3}", score.Score, score.Team.Name, score.Game.Week.Name, score.Game.Week.Season.Year, index++);
            }
        }

        private static void GetTeamsWithMost1000PointGames(League leagueData, int count = 10)
        {
            var scores = leagueData.Seasons.SelectMany(s => s.Weeks)
                               .SelectMany(w => w.Games)
                               .SelectMany(g => new[] { g.Home, g.Away })
                               .Where(s => s.Score >= 1000);

            var groupedScores = scores.GroupBy(g => g.Team).OrderByDescending(x => x.Count())
                               .Take(count);

            var index = 1;
            foreach (var team in groupedScores)
            {
                Console.WriteLine("{2,2}. {1,-5} {0}", team.Key.Name, team.Count(), index++);
            }
        }

        private static void GetTeamsAndYearsPlayed(League leagueData)
        {
            foreach (Team team in leagueData.Teams.OrderBy(t => t.Name))
            {
                var grouping = team.Games.GroupBy(g => g.Week.Season);
                var array = grouping.Select(g => g.Key.Year);

                Console.WriteLine("{0} - {1} years", team.Name, array.Count());
                Console.WriteLine("  {0}", string.Join(", ", array));
                Console.WriteLine("");

            }
        }

        private static void GetTeamsAndPlayoffAppearances(League leagueData)
        {
            foreach (Team team in leagueData.Teams.OrderBy(t => t.Name))
            {
                var grouping = team.Games.Where(g => !g.Week.Name.StartsWith("Week")).GroupBy(g => g.Week.Season);
                var array = grouping.Select(g => g.Key.Year);
                if (array.Count() == 0) continue;
                

                Console.WriteLine("{0} - {1} years", team.Name, array.Count());
                Console.WriteLine("  {0}", string.Join(", ", array));
                Console.WriteLine("");

            }
        }

        private static void GetChampionships(League leagueData)
        {
            Dictionary<Team, List<int>> cache = new Dictionary<Team, List<int>>();

            foreach (var season in leagueData.Seasons)
            {
                var bowl = season.Playoffs.Single(w => w.Name.StartsWith("Fantasy Bowl")).Games.SingleOrDefault();
                if (bowl == null)
                    break;

                Team champion = bowl.GetWinningTeam();
                Console.WriteLine("{0} {1}", season.Year, champion.Name);

                List<int> years;
                bool exists = cache.TryGetValue(champion, out years);
                if (!exists)
                {
                    years = new List<int>();
                    cache.Add(champion, years);
                }
                years.Add(season.Year);
            }

            Console.WriteLine();
            Console.WriteLine();

            foreach (var item in cache.OrderByDescending(x => x.Value.Count))
            {
                Console.WriteLine("{0, -30} {1}", item.Key.Name, string.Join(", ", item.Value));
            }

        }

        private static void GetAverages(League leagueData)
        {
            {
                var seasonGames = leagueData.Seasons.SelectMany(s => s.Weeks).SelectMany(w => w.Games).ToArray();
                var playoffGames = leagueData.Seasons.SelectMany(s => s.Playoffs).SelectMany(w => w.Games).ToArray();

                double avgSeasonWin = seasonGames.Average(x => x.GetWinningScore().Score);
                double avgSeasonLosing = seasonGames.Average(x => x.GetLosingScore().Score);
                double avgSeasonScore = seasonGames.SelectMany(g => new[] { g.Home, g.Away }).Average(s => s.Score);

                double avgPlayoffWin = playoffGames.Average(x => x.GetWinningScore().Score);
                double avgPlayoffLosing = playoffGames.Average(x => x.GetLosingScore().Score);
                double avgPlayoffScore = playoffGames.SelectMany(g => new[] { g.Home, g.Away }).Average(s => s.Score);


                Console.WriteLine("{0,-15} {1,-7} {2,-7} {3,-7}", "WaFFL", "Avg", "Avg W", "Avg L");
                Console.WriteLine("{0,15}+{1,-5}+{2,-5}+{3,-5}", new String('-', 15), new String('-', 7), new String('-', 7),new String('-', 7));
                Console.WriteLine("{0,-15} {1,7:0.0} {2,7:0.0} {3,7:0.0}", "Season", avgSeasonScore, avgSeasonWin, avgSeasonLosing);
                Console.WriteLine("{0,-15} {1,7:0.0} {2,7:0.0} {3,7:0.0}", "Playoffs", avgPlayoffScore, avgPlayoffWin,avgPlayoffLosing);
            }

            Console.WriteLine("");

            foreach (Season season in leagueData.Seasons)
            {
                var seasonGames = season.Weeks.SelectMany(w => w.Games).ToArray();
               
                double avgSeasonWin = seasonGames.Average(x => x.GetWinningScore().Score);
                double avgSeasonLosing = seasonGames.Average(x => x.GetLosingScore().Score);
                double avgSeasonScore = seasonGames.SelectMany(g => new[] { g.Home, g.Away }).Average(s => s.Score);

                Console.WriteLine("{0,-15} {1,7:0.0} {2,7:0.0} {3,7:0.0}", "Season " + season.Year, avgSeasonScore, avgSeasonWin, avgSeasonLosing);
            }

            Console.WriteLine();
        }
        
        private static void GetAverages(Team team)
        {
            {
                var seasonGames = team.Games.Where(g => g.Week.Name.StartsWith("Week ")).ToArray();
                var playoffGames = team.Games.Where(g => !g.Week.Name.StartsWith("Week")).ToArray(); ;

                double avgSeasonWin = seasonGames.Where(g => g.IsWinningTeam(team)).AverageSafe(x => x.GetWinningScore().Score);
                double avgSeasonLosing = seasonGames.Where(g => g.IsLosingTeam(team)).AverageSafe(x => x.GetLosingScore().Score);
                double avgSeasonScore = seasonGames.Select(g => g.GetTeamScore(team)).AverageSafe();

                double avgPlayoffWin = playoffGames.Where(g => g.IsWinningTeam(team)).AverageSafe(x => x.GetWinningScore().Score);
                double avgPlayoffLosing = playoffGames.Where(g => g.IsLosingTeam(team)).AverageSafe(x => x.GetLosingScore().Score);
                double avgPlayoffScore = playoffGames.Select(g => g.GetTeamScore(team)).AverageSafe();

                string displayName = team.Name;
                if (displayName.Length > 15)
                {
                    displayName = displayName.Substring(0, 15);
                }

                Console.WriteLine("{0,-15} {1,-7} {2,-7} {3,-7}", displayName, "Avg", "Avg W", "Avg L");
                Console.WriteLine("{0,15}+{1,-5}+{2,-5}+{3,-5}", new String('-', 15), new String('-', 7), new String('-', 7), new String('-', 7));
                Console.WriteLine("{0,-15} {1,7:0.0} {2,7:0.0} {3,7:0.0}", "Season", avgSeasonScore, avgSeasonWin, avgSeasonLosing);
                Console.WriteLine("{0,-15} {1,7:0.0} {2,7:0.0} {3,7:0.0}", "Playoffs", avgPlayoffScore, avgPlayoffWin, avgPlayoffLosing);
            }

            Console.WriteLine("");

            var seasonGroups = team.Games.GroupBy(g => g.Week.Season);
            foreach (var games in seasonGroups)
            {
                var seasonGames = games.ToArray();

                double avgSeasonWin = seasonGames.Where(g => g.IsWinningTeam(team)).AverageSafe(x => x.GetWinningScore().Score);
                double avgSeasonLosing = seasonGames.Where(g => g.IsLosingTeam(team)).AverageSafe(x => x.GetLosingScore().Score);
                double avgSeasonScore = seasonGames.Select(g => g.GetTeamScore(team)).AverageSafe();

                Console.WriteLine("{0,-15} {1,7:0.0} {2,7:0.0} {3,7:0.0}", "Season " + games.Key.Year, avgSeasonScore, avgSeasonWin, avgSeasonLosing);
            }

            Console.WriteLine();
        }

        private static void GetHeadToHeadRecordsFor(Team teamData)
        {
            var grouping = teamData.Games.GroupBy(g => g.OpponentOf(teamData)).OrderByDescending(g => g.Count());

            foreach (var group in grouping)
            {
                int wins = group.Where(g => g.Week.Name.StartsWith("Week ")).Count(g => g.GetWinningTeam() == teamData);
                int losses = group.Where(g => g.Week.Name.StartsWith("Week ")).Count(g => g.GetLosingTeam() == teamData);

                double winPercentage = (double)wins / (double)(wins + losses);
                winPercentage *= 100.0;


                int pwins = group.Where(g => !g.Week.Name.StartsWith("Week ")).Count(g => g.GetWinningTeam() == teamData);
                int plosses = group.Where(g => !g.Week.Name.StartsWith("Week ")).Count(g => g.GetLosingTeam() == teamData);

                Console.WriteLine("{3,-30} {0,3}-{1,3}  {2,4:0.0}%       ({4,3}-{5,3})", wins, losses, winPercentage, group.Key.Name, pwins, plosses);
            }

        }

        private static void GetHighestScoringGames(League leagueData, int count = 10)
        {
            var orderedGames = leagueData.Seasons.SelectMany(s => s.Weeks)
                                           .SelectMany(w => w.Games)
                                           .OrderByDescending(g => g.Home.Score + g.Away.Score)
                                           .Take(count);

            int index = 1;
            foreach (var game in orderedGames)
            {
                Console.WriteLine("{6,2}. {0,4}-{1,-4}   {2} vs {3}   {4} {5}", game.Home.Score, game.Away.Score, game.Home.Team.Name, game.Away.Team.Name, game.Week.Name, game.Week.Season.Year, index++);
            }
        }
    }
}

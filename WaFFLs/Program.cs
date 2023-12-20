using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WaFFLs.Generation;

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
            //for (int i = 1996; i <= 2023; i++)
            //    UpdateYear(2023);

            League leagueData = new League();

            IWaFFLDataSource dataProvider = new CachedWaFFLDataSource();
            ITeamResolver teamResolver = new LeagueTeamResolver(leagueData);
            var parser = new HtmlPageParser(dataProvider, teamResolver);
            parser.Parse(leagueData, 1996, 2023);

            var e = new Engine(leagueData, "c:\\waffles-output");

            var providers =
                typeof(Program).Assembly.GetTypes()
                    .Where(t => !t.IsInterface && typeof(IProvider).IsAssignableFrom(t))
                    .Select(t => (object)Activator.CreateInstance(t, leagueData, 25))
                    .ToList();

            e.Generate(providers);

            Process.Start("c:\\waffles-output\\index.htm");


            //GetAllTeamsEverInLeague(leagueData);
            GetAllTeamsInSeason(leagueData, 2023);
            //GetTeamsAndYearsPlayed(leagueData);
            //GetTeamsAndPlayoffAppearances(leagueData);
            //GetAverages(leagueData);
            //var team = leagueData.Teams.Single(t => t.Name == "Rocky Mountain Oysters");
            //GetAverages(leagueData);

            Console.WriteLine("Completed");
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
                var seasonGames = team.Games.Where(g => g.Week.IsRegular()).ToArray();
                var playoffGames = team.Games.Where(g => g.Week.IsPlayoff()).ToArray(); ;

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
    }
}

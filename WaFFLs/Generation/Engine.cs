using System;
using RazorEngine.Templating;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RazorEngine.Configuration;
using WaFFLs.Generation.Models;
using WaFFLs.Generation.Views;

namespace WaFFLs.Generation
{
    public class Engine
    {
        private readonly League _leagueData;
        private readonly string _root;

        public Engine(League leagueData, string root)
        {
            _leagueData = leagueData;
            _root = root;
            if (!_root.EndsWith("\\"))
            {
                _root += "\\";
            }
        }

        public void Generate(List<object> providers)
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, true);
            }

            Directory.CreateDirectory(_root);

            var config = new TemplateServiceConfiguration()
            {
                DisableTempFileLocking = false,
                CachingProvider = new DefaultCachingProvider(t => { }),
            };

            using (IRazorEngineService razor = RazorEngineService.Create(config))
            {
                CacheViews(razor);
                GenerateHome(razor, providers);
                GenerateTeams(razor);
                GenerateRecords<ICareerRecordProvider>(razor, "career-record-list", providers, x => x.GetData());
                GenerateRecords<ISeasonRecordProvider>(razor, "season-record-list", providers, x => x.GetData());
                GenerateRecords<IGameRecordProvider>(razor, "game-record-list", providers, x => x.GetData());
                GenerateRecords<IIndividualGameRecordProvider>(razor, "individual-game-record-list", providers, x => x.GetData());
                GenerateRecords<IStreakRecordProvider>(razor, "streak-record-list", providers, x => x.GetData());
            }
        }

        private void GenerateHome(IRazorEngineService razor, List<object> providers)
        {
            using (var writer = new StreamWriter(_root + "index.htm"))
            {
                object model = providers.Select(p => p.GetType()).OrderBy(t => t.Name).ToList();
                razor.RunCompile("home", writer, null, model);
            }
        }

        private void GenerateRecords<T>(IRazorEngineService razor, string template, List<object> providers, Func<T, object> modelResolver)
        {
            foreach (var recordProvider in providers.OfType<T>())
            {
                using (var writer = new StreamWriter(_root + recordProvider.GetType().Name + ".htm"))
                {
                    object model = modelResolver(recordProvider);

                    var viewBag = new DynamicViewBag();
                    viewBag.AddValue("Title", TitleAttribute.GetTitleText(recordProvider));
                    viewBag.AddValue("Summary", SummaryAttribute.GetSummaryText(recordProvider));
                    
                    razor.RunCompile(template, writer, null, model, viewBag);
                }
            }
        }

        private void CacheViews(IRazorEngineService razor)
        {
            razor.AddTemplate("team-list", View.Get("TeamList.cshtml"));
            razor.AddTemplate("team-info", View.Get("Team.cshtml"));
            razor.AddTemplate("individual-game-record-list", View.Get("IndividualGameRecordList.cshtml"));
            razor.AddTemplate("game-record-list", View.Get("GameRecordList.cshtml"));
            razor.AddTemplate("season-record-list", View.Get("SeasonRecordList.cshtml"));
            razor.AddTemplate("career-record-list", View.Get("CareerRecordList.cshtml"));
            razor.AddTemplate("streak-record-list", View.Get("StreakRecordList.cshtml"));
            razor.AddTemplate("home", View.Get("Home.cshtml"));
        }

        private void GenerateTeams(IRazorEngineService razor)
        {
            var teams = GetTeamInfo(_leagueData);

            using (var writer = new StreamWriter(_root + "teams.htm"))
            {
                razor.RunCompile("team-list", writer, null, teams.OrderBy(t => t.Name).ToList());
            }

            foreach (var team in teams)
            {
                using (var writer = new StreamWriter(_root + team.Filename))
                {
                    razor.RunCompile("team-info", writer, null, team);
                }
            }
        }






















        public List<TeamInfo> GetTeamInfo(League leagueData)
        {
            var teams = _leagueData.Teams.Select(t => new TeamInfo
            {
                Name = t.Name,
                Owner = t.Owner,
                FirstSeason = t.Games.Min(g => g.Week.Season.Year),
                LastSeason = t.Games.Max(g => g.Week.Season.Year),
                Filename = GetSafeFilename(t.Name + ".htm"),
                SeasonRecord = GetSeasonRecordForTeam(t),
                PlayoffRecord = GetPlayoffRecordForTeam(t),
                HeadToHeadRecords = GetHeadToHeadRecordsFor(t),
                Seasons = GetAllGamesBySeasonForTeam(t),
            }).ToList();





            return teams;
        }

        private static List<TeamStanding> GetHeadToHeadRecordsFor(Team teamData)
        {
            var results = new List<TeamStanding>();

            var grouping = teamData.Games.GroupBy(g => g.OpponentOf(teamData)).OrderByDescending(g => g.Count());
            foreach (var group in grouping)
            {
                int wins = group.Where(g => g.Week.Name.StartsWith("Week ")).Count(g => g.GetWinningTeam() == teamData);
                int losses = group.Where(g => g.Week.Name.StartsWith("Week ")).Count(g => g.GetLosingTeam() == teamData);

                int pwins = group.Where(g => !g.Week.Name.StartsWith("Week ")).Count(g => g.GetWinningTeam() == teamData);
                int plosses = group.Where(g => !g.Week.Name.StartsWith("Week ")).Count(g => g.GetLosingTeam() == teamData);

                var r = new TeamStanding()
                {
                    Team = group.Key,
                    SeasonRecord = new Record() { Wins = wins, Losses = losses },
                    PlayoffRecord = new Record() { Wins = pwins, Losses = plosses },
                };
                results.Add(r);
            }

            return results;
        }

        private static Record GetSeasonRecordForTeam(Team team)
        {
            int wins = team.Games.Where(g => g.Week.Name.StartsWith("Week ")).Count(g => g.GetWinningTeam() == team);
            int losses = team.Games.Where(g => g.Week.Name.StartsWith("Week ")).Count(g => g.GetLosingTeam() == team);

            return new Record() { Wins = wins, Losses = losses };
        }

        private static Record GetPlayoffRecordForTeam(Team team)
        {
            int wins = team.Games.Where(g => !g.Week.Name.StartsWith("Week ")).Count(g => g.GetWinningTeam() == team);
            int losses = team.Games.Where(g => !g.Week.Name.StartsWith("Week ")).Count(g => g.GetLosingTeam() == team);

            return new Record() { Wins = wins, Losses = losses };
        }



        private static List<SeasonInfo> GetAllGamesBySeasonForTeam(Team team)
        {
            var results = new List<SeasonInfo>();

            var seasonGroups = team.Games.GroupBy(g => g.Week.Season).OrderByDescending(g => g.Key.Year);
            foreach (var games in seasonGroups)
            {
                var regularGames = games.Where(g => g.Week.Name.StartsWith("Week ")).ToList();

                int wins = regularGames.Count(g => g.GetWinningTeam() == team);
                int losses = regularGames.Count(g => g.GetLosingTeam() == team);

                bool valid = (wins + losses == 15 && games.Key.Year == 1996) || (wins + losses == 14 && games.Key.Year != 1996);
                if (!valid)
                {
                    double winPercentage = (double)wins / (double)(wins + losses);
                    winPercentage *= 100;
                    Console.WriteLine("{3} {0} - {1,5} {2,10:00.0}%", games.Key.Year, $"{wins}-{losses}*", winPercentage, team.Name);
                }
                var s = new SeasonInfo()
                {
                    Name = games.Key.Year.ToString(),
                    Record = new Record() {  Wins = wins, Losses = losses },
                    Games = new List<GameInfo>(),
                };
                results.Add(s);


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

                    var g = new GameInfo()
                    {
                        Date = game.Week,
                        Opponent = theirScore.Team,
                        OpponentScore = theirScore.Score,
                        Score = myScore.Score,
                    };
                    s.Games.Add(g);
                }
            }

            return results;
        }

        public string GetSafeFilename(string filename)
        {
            Directory.CreateDirectory(_root + "teams");


            return "teams/" + string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));

        }

    }
}

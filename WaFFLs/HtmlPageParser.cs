using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WaFFLs
{
    public class HtmlPageParser
    {
        private readonly IWaFFLDataSource _dataSource;

        private readonly TableDataParser _seasonParser;
        private readonly TableDataParser _postseasonParser;
        private readonly GameExtractor _extractor;
        private readonly PlayoffGameExtractor _playoffExtractor;

        public HtmlPageParser(IWaFFLDataSource dataSource, ITeamResolver teamResolver)
        {
            _dataSource = dataSource;
            _seasonParser = new TableDataParser("Week 1");
            _postseasonParser = new TableDataParser("Wild Card Week", "Wild Card", "Semi-Finals");
            _extractor = new GameExtractor(teamResolver);
            _playoffExtractor = new PlayoffGameExtractor(teamResolver);
        }

        public void Parse(League leagueData, int startYear, int endYear)
        {
            for (int year = startYear; year <= endYear; year++)
            {
//                Console.WriteLine("{0}", year);

                Season seasonData = new Season() { Year = year };
                leagueData.Seasons.Add(seasonData);
                var raw = _dataSource.GetStandingsDataForYear(year);
                ParseRegularSeason(raw, year, seasonData);
                ParsePostSeason(raw, year, seasonData);
            }
        }

        private void ParseRegularSeason(string raw, int year, Season seasonData)
        {
            string seasonTable = _seasonParser.GetData(raw);
            seasonTable = seasonTable.Replace("&nbsp;", " ");
            XElement season = XElement.Parse(seasonTable);
            _extractor.Extract(season, year, seasonData);
        }

        public void ParsePostSeason(string raw, int year, Season seasonData)
        {
            string postseasonTable = _postseasonParser.GetData(raw);
            postseasonTable = postseasonTable.Replace("&nbsp;", " ");
            XElement postseason = XElement.Parse(postseasonTable);
            _playoffExtractor.Extract(postseason, year, seasonData);
        }
    }

    public class PlayoffGameExtractor
    {
        private ITeamResolver _teamResolver;

        public PlayoffGameExtractor(ITeamResolver teamResolver)
        {
            _teamResolver = teamResolver;
        }

        public void Extract(XElement postseason, int year, Season seasonData)
        {
            var weeks = postseason.Element("tbody").Elements("tr").ToArray();

            if ((year == 2021 || year == 2022) && weeks.Length == 5)
            {
                // an extra element was added in 2021
                weeks = weeks.Take(weeks.Length - 1).ToArray();
            }

            for (int i = 0; i < weeks.Length; i += 2)
            {
                var headers = weeks[i].Elements("td").ToArray();
                if (headers.Length == 0)
                {
                    headers = weeks[i].Elements("th").ToArray();
                }
                var games = weeks[i + 1].Elements("td").ToArray();

                for (int j = 0; j < games.Length; j++)
                {
                    string text = headers[j].Value.Trim();
                    if (string.IsNullOrEmpty(text))
                        continue;

                    var weekData = new Week() { Name = text, Season = seasonData };
                    seasonData.Playoffs.Add(weekData);

                    var individualGames = games[j].Elements("div").ToArray();
                    if (individualGames.Length == 0 && year == 2021)
                    {
                        if (text == "Fantasy Bowl XXVI")
                        {
                            individualGames = new XElement[] { XElement.Parse("<div>Marauding Nomads 1306, Sporky's Revenge 713</div>") };
                        }
                        else
                        {
                            individualGames = games[j].Elements("font").ToArray();
                        }
                    }

                    foreach (var g in individualGames)
                    {
                        if (g.Value.StartsWith("AFC:") || g.Value.StartsWith("NFC:"))
                        {
                            g.Value = g.Value.Substring(4);
                        }
                    }

                    foreach (var ig in individualGames)
                    {
                        string line = ig.Value;
                        if (string.IsNullOrEmpty(line)) continue;
                        if (line.StartsWith("MVP:")) continue;
                        

                        ProcessGameLine(line, weekData);
                    }
                }
            }
        }

        private void ProcessGameLine(string line, Week weekData)
        {
            string[] raw = line.Split(',');
            if (raw.Length == 2)
            {
                Game gameData = new Game();
                gameData.Week = weekData;
                weekData.Games.Add(gameData);

                TeamScore homeScoreData = new TeamScore();
                homeScoreData.Game = gameData;
                ProcessTeam(raw[0], homeScoreData, 0);
                gameData.Home = homeScoreData;

                TeamScore awayScoreData = new TeamScore();
                awayScoreData.Game = gameData;
                ProcessTeam(raw[1], awayScoreData, 1);
                gameData.Away = awayScoreData;
            }
            else
            {
                if (line.Contains("@") || line.Contains(" v ") || line.Contains(" v. "))
                    return;

                throw new Exception();
            }
        }

        private void ProcessTeam(string teamline, TeamScore scoreData, int pos)
        {
            string raw = teamline.Trim();

            int scoreIndex = raw.Length;
            while (char.IsDigit(raw[--scoreIndex])) ;
            scoreIndex++;

            string team = raw.Substring(0, scoreIndex).Trim();
            string score = raw.Substring(scoreIndex).Trim();

            int scoreInt = Convert.ToInt32(score);

            Team teamData = _teamResolver.GetTeamByName(team);
            teamData.Games.Add(scoreData.Game);

            scoreData.Team = teamData;
            scoreData.Score = scoreInt;
        }
    }

    public class GameExtractor
    {
        private ITeamResolver teamResolver;

        public GameExtractor(ITeamResolver resolver)
        {
            teamResolver = resolver;
        }

        public void Extract(XElement season, int year, Season seasonData)
        {
            var weeks = season.Element("tbody").Elements("tr").ToArray();

            int start = 0;

            if (year == 2011)
            {
                start = 1;

                var specialGames = weeks[0].Elements("td");
                foreach (var game in specialGames)
                {
                    var lines = game.Elements("div").ToArray();

                    string text = lines[0].Value.Split('-')[0].Trim();

                    var weekData = new Week() { Name = text, Season = seasonData };
                    seasonData.Weeks.Add(weekData);

                    for (int i = 1; i < lines.Length; i++)
                    {
                        string line = lines[i].Value;
                        ProcessGameLine(line, weekData);
                    }
                }
            }


            for (int i = start; i < weeks.Length; i += 2)
            {
                var headers = weeks[i].Elements("td").ToArray();
                var games = weeks[i + 1].Elements("td").ToArray();

                for (int j = 0; j < games.Length; j++)
                {
                    string text = headers[j].Value.Split('-')[0].Trim();
                    if (string.IsNullOrEmpty(text))
                        continue;

                    var weekData = new Week() { Name = text, Season = seasonData };
                    seasonData.Weeks.Add(weekData);

//                    Console.WriteLine("  " + text);

                    var individualGames = games[j].Elements("div").ToArray();
                    if (individualGames.Length == 1)
                    {
                        int count = individualGames.Single().Elements().Count();
                        if (individualGames.Single().Elements().All(x => x.Name == "font"))
                        {
                            var unwrappedItems = individualGames.Single().Elements().SelectMany(x => x.Elements());
                            var newwrapper = new XElement("temp", unwrappedItems);
                            individualGames = new[] { newwrapper };
                        }
                        else if (count == 1 && year == 2019 && text == "Week 8")
                        {
                            // lots of nested div's for this week. 
                            individualGames = individualGames.Single().Elements("div").Elements("div").ToArray();
                        }
                        else if (count == 1)
                        {
                            individualGames = individualGames.Elements("font").ToArray();
                        }
                        else if (count == 4)
                        {
                            var parent = individualGames.Single();
                            var items = parent.Elements().ToList();
                            foreach (var item in items)
                            {
                                item.Remove();
                                foreach (var child in item.Elements())
                                {
                                    parent.Add(child);
                                }
                            }
                        }

                        individualGames = individualGames.Elements("div").ToArray();
                    }
                    else if (individualGames.Length == 2)
                    {
                        var x = individualGames[0];
                        var y = individualGames[1].Elements("font").Elements("div");

                        var innerText = individualGames[1].Nodes().OfType<XText>().Select(t => t.Value);
                        var fakeNode = new XElement("div", innerText);

                        List<XElement> temp = new List<XElement>(y);
                        temp.Add(x);
                        temp.Add(fakeNode);

                        individualGames = temp.ToArray();
                    }

                    if (year == 2006 && weekData.Name == "Week 13")
                    {
                        // Data is screwed up in week 13, 2006.  It appears that
                        // teams were playing the other division in the conference
                        // there for, Drunken Squirrels must of played the Ultracogs
                        // since the 'cogs shared a division with Team Bean.
                        //
                        // Team Bean[DS] 709, Ultracogs 417

                        foreach (var element in individualGames)
                        {
                            if (element.Value == "Team Bean 709, Ultracogs 417")
                            {
                                element.Value = "Drunken Squirrels 709, Ultracogs 417";
                            }
                        }
                    }

                    if (year == 2007 && weekData.Name == "Week 6")
                    {
                        // Data as screwed up in week 6, 2007.  After careful analysis,
                        // I determined that the following games were in need of correction
                        // with the team in [] being the actual team that should of been
                        // recorded
                        //
                        // Wolves[BB] 443, Wolfins 177
                        // Fighting Calrissians[TB] 1095, Sporky's Revenge 1035
                        // Eternals[RMO] 520, Ultracogs 469
                        // TD Matrix 736, Dominators[MN] 390

                        foreach (var element in individualGames)
                        {
                            if (element.Value == "Wolves 443, Wolfins 177")
                            {
                                element.Value = "Bayou Boys 443, Wolfins 177";
                            }
                            else if (element.Value == "Fighting Calrissians 1095, Sporky's Revenge 1035")
                            {
                                element.Value = "Team Bean 1095, Sporky's Revenge 1035";
                            }
                            else if (element.Value == "Eternals 520, Ultracogs 469")
                            {
                                element.Value = "Rocky Mountain Oysters 520, Ultracogs 469";
                            }
                            else if (element.Value == "TD Matrix 736, Dominators 390")
                            {
                                element.Value = "TD Matrix 736, Marauding Nomads 390";
                            }
                        }
                    }

                    if (year == 2015 && weekData.Name == "Week 11")
                    {
                        // score mis-recorded as 10,069 - assuming 1,069

                        foreach (var element in individualGames)
                        {
                            if (element.Value == "Dont Tase Me Bro 10069, Rocky Mountain Oysters 907")
                            {
                                element.Value = "Dont Tase Me Bro 1069, Rocky Mountain Oysters 907";
                            }
                        }
                    }

                    if (year == 2016 && weekData.Name == "Week 2")
                    {
                        // two scores were recorded in a single cell, so adding an additional cell with
                        // the second value

                        foreach (var element in individualGames)
                        {
                            if (element.Value == "Speed Demons 1015, Bayou Boys 971 \nWolves 846, Monica Loves Clinton Dix 732")
                            {
                                element.Value = "Speed Demons 1015, Bayou Boys 971";
                            }
                        }

                        int size = individualGames.Length;
                        Array.Resize(ref individualGames, size + 1);
                        individualGames[size] = XElement.Parse("<div align=\"left\">Wolves 846, Monica Loves Clinton Dix 732</div>");
                    }

                    if (year == 2019 && weekData.Name == "Week 10")
                    {
                        // dominators had a special character infront of their score.

                        foreach (var element in individualGames)
                        {
                            if (element.Value == "Koothrapaulli Browns 915, Dominators @742 ")
                            {
                                element.Value = "Koothrapaulli Browns 915, Dominators 742";
                            }
                        }
                    }

                    if (year == 2021 && weekData.Name == "Week 12")
                    {
                        // Rocky mountian oysters is mis-spelled

                        foreach (var element in individualGames)
                        {
                            if (element.Value == "Sporky's Revenge 1043, Rocky Mouyntain Oysters 369 ")
                            {
                                element.Value = "Sporky's Revenge 1043, Rocky Mountain Oysters 369";
                            }
                        }
                    }

                    foreach (var ig in individualGames)
                    {
                        string line = ig.Value;
                        ProcessGameLine(line, weekData);
                    }
                }
            }
        }

        private void ProcessGameLine(string line, Week weekData)
        {
            if (string.IsNullOrEmpty(line))
                return;

            string[] raw = line.Split(',');
            if (raw.Length == 2)
            {
                Game gameData = new Game();
                gameData.Week = weekData;
                weekData.Games.Add(gameData);

                TeamScore homeScoreData = new TeamScore();
                homeScoreData.Game = gameData;
                ProcessTeam(raw[0], homeScoreData, 0);
                gameData.Home = homeScoreData;

                TeamScore awayScoreData = new TeamScore();
                awayScoreData.Game = gameData;
                ProcessTeam(raw[1], awayScoreData, 1);
                gameData.Away = awayScoreData;
            }
            else
            {
                int x = 0;
                while (!char.IsDigit(line[x++])) ;
                while (char.IsDigit(line[x++])) ;

                string newline = line.Substring(0, x) + "," + line.Substring(x);
                ProcessGameLine(newline, weekData);
            }
        }

        private void ProcessTeam(string teamline, TeamScore scoreData, int pos)
        {
            string raw = teamline.Trim();

            int scoreIndex = raw.Length;
            while (char.IsDigit(raw[--scoreIndex])) ;
            scoreIndex++;

            string team = raw.Substring(0, scoreIndex).Trim();
            string score = raw.Substring(scoreIndex).Trim();

            if (scoreData.Game.Week.Name == "Week 12" && scoreData.Game.Week.Season.Year == 2008 &&
                score == "" && team == "Fighting Calrissians")
            {
                // there is no score data for FC this week, so this was calculated by totaling the
                // results of all games and subtracting the seaons points for.
                score = "819";
            }

            if (scoreData.Game.Week.Name == "Week 1" && scoreData.Game.Week.Season.Year == 2003 &&
                team == "Phantom")
            {
                // in 2003 the team was called the Phantoms, but was recorded as
                // phantom
                team = "Phantom120";
            }

            int scoreInt = Convert.ToInt32(score);

            Team teamData = teamResolver.GetTeamByName(team);
            teamData.Games.Add(scoreData.Game);

            scoreData.Team = teamData;
            scoreData.Score = scoreInt;
        }
    }

    public class TableDataParser
    {
        private const string TableStart = "<table ";
        private const string TableEnd = "</table>";

        private readonly string[] keywords;

        public TableDataParser(params string[] keywords)
        {
            this.keywords = keywords;
        }

        public string GetData(string raw)
        {
            int index = -1;
            foreach (string keyword in this.keywords)
            {
                index = raw.IndexOf(keyword, StringComparison.Ordinal);
                if (index != -1)
                {
                    break;
                }
            }

            if (index == -1) throw new Exception();

            string trunc = raw.Substring(0, index);
            int start = trunc.LastIndexOf(TableStart, index, StringComparison.Ordinal);
            int end = raw.IndexOf(TableEnd, start, StringComparison.Ordinal) + TableEnd.Length;
            string tableText = raw.Substring(start, end - start);
            return tableText;
        }
    }
}
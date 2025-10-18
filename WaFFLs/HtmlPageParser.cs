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
                Console.WriteLine("Parsing {0}", year);

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
            seasonTable = CleanupTableData(seasonTable);
            XElement season = XElement.Parse(seasonTable);
            _extractor.Extract(season, year, seasonData);
        }

        private void ParsePostSeason(string raw, int year, Season seasonData)
        {
            string postseasonTable = _postseasonParser.GetData(raw);
            postseasonTable = CleanupTableData(postseasonTable);
            XElement postseason = XElement.Parse(postseasonTable);
            _playoffExtractor.Extract(postseason, year, seasonData);
        }

        private string CleanupTableData(string data)
        {
            return data.Replace("&nbsp;", " ")
                       .Replace("Â", "")
                       .Replace(" ", " ");    // unicode 160 with unicode 32
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

            if (year >= 2021 && weeks.Length == 5)
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
                    if (individualGames.Length == 0 && year >= 2021)
                    {
                        if (text == "Fantasy Bowl XXVI")
                        {
                            individualGames = new XElement[] { XElement.Parse("<div>Marauding Nomads 1306, Sporky's Revenge 713</div>") };
                        }
                        else if (text == "Fantasy Bowl XXVII")
                        {
                            individualGames = new XElement[] { XElement.Parse("<div>Ultracogs 826, Truffle Shuffle 795</div>") };
                        }
                        else if (text == "Fantasy Bowl XXVIII")
                        {
                            individualGames = new XElement[] { XElement.Parse("<div>Fantasy Cognoscenti 1422, Rocky Mountain Oysters 958</div>") };
                        }
                        else if (text == "Fantasy Bowl XXIX")
                        {
                            individualGames = new XElement[] { XElement.Parse("<div>Sporky's Revenge 1080, Truffle Shuffle 825</div>") };
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
            if (string.IsNullOrWhiteSpace(line))
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

            int nameIndex = 0;
            if (raw.StartsWith("(#"))
            {
                while (!char.IsWhiteSpace(raw[++nameIndex])) ;
            }

            string team = raw.Substring(nameIndex, scoreIndex-nameIndex).Trim();
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

                    if (year == 2003 && weekData.Name == "Week 1")
                    {
                        // Data is screwed up in week 1, 2013.  The Phantoms120 and Nuclear181
                        // were given no score that week. The new scored were calculated by
                        // totaling the results of all other games and subtracting the seasons
                        // points for.

                        foreach (var element in individualGames)
                        {
                            if (element.Value == "Phantom120, Rocky Mountain Oysters 382")
                            {
                                element.Value = "Phantom120 572, Rocky Mountain Oysters 382";
                            }
                            else if (element.Value == "Nuclear181, Team Bean 441")
                            {
                                element.Value = "Nuclear181 1018, Team Bean 441";
                            }
                        }
                    }

                    if (year == 2006 && weekData.Name == "Week 13")
                    {
                        // Data is screwed up in week 13, 2006.  It is nearly a copy of
                        // week 11.  It is an interdivision week and should be matched
                        // with Week 2.  Double checked with week 12 & 13 to ensure no
                        // overlap.  Scores were computed by diffing remingn 13 weeks with 
                        // points for. Assuming TB & UC core is correct, since it is a delta with
                        // week 11 - TB matched perfectly, UC was off by 150ish..  BB scored 1757
                        // via calculation, which is an enormous score, so assuming a -150pt
                        // mistake - since this would be a top 10 score 
                        //
                        // [Correct]  Team Bean 709, Ultracogs 417       
                        // [Adjusted] Wolfins 782, Overpaid Crybabies 1338
                        // [Adjusted] Marauding Nomads 670, Eternals 1296
                        // [Adjusted] Wolves 782, Drunken Squirrels 1081
                        // [Adjusted] Bayou Boys 1607, Dominators 523
                        // [Adjusted] Sporky's Revenge 809, Red Raiders 1177
                        // [Adjusted] Fighting Calrissians 735, TD Matrix 363
                        // [Adjusted] Phantoms 727, Rocky Mountain Oysters 888

                        foreach (var element in individualGames)
                        {
                            if (element.Value == "Overpaid Crybabies 1382, Marauding Nomads 1113")
                            {
                                element.Value = "Wolfins 782, Overpaid Crybabies 1338";
                            }
                            else if (element.Value == "Team Bean 661, Wolves 510")
                            {
                                element.Value = "Marauding Nomads 670, Eternals 1296";
                            }
                            else if (element.Value == "Wolfins 752, Eternals 421")
                            {
                                element.Value = "Wolves 782, Drunken Squirrels 1081";
                            }
                            else if (element.Value == "Dominators 681, Phantoms 488")
                            {
                                element.Value = "Bayou Boys 1607, Dominators 523";
                            }
                            else if (element.Value == "Sporky's Revenge 888, TD Matrix 500")
                            {
                                element.Value = "Sporky's Revenge 809, Red Raiders 1177";
                            }
                            else if (element.Value == "Bayou Boys 1008, Rocky Mountain Oysters 779")
                            {
                                element.Value = "Fighting Calrissians 735, TD Matrix 363";
                            }
                            else if (element.Value == "Fighting Calrissians 1448, Red Raiders 423")
                            {
                                element.Value = "Phantoms 727, Rocky Mountain Oysters 888";
                            }
                        }
                    }

                    if (year == 2008 && weekData.Name == "Week 12")
                    {
                        // there is no score data for FC this week, so this was calculated by totaling the
                        // results of all games and subtracting the seaons points for.

                        foreach (var element in individualGames)
                        {
                            if (element.Value == "Dominators 1170, Fighting Calrissians")
                            {
                                element.Value = "Dominators 1170, Fighting Calrissians 819";
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
                        // rocky mountian oysters is mis-spelled

                        foreach (var element in individualGames)
                        {
                            if (element.Value == "Sporky's Revenge 1043, Rocky Mouyntain Oysters 369 ")
                            {
                                element.Value = "Sporky's Revenge 1043, Rocky Mountain Oysters 369";
                            }
                        }
                    }

                    if (year == 2024 && weekData.Name == "Week 13")
                    {
                        // koothrapaulli browns score is a super high, assuming 851 is the correct score

                        foreach (var element in individualGames)
                        {
                            if (element.Value == "Koothrapaulli Browns 8518, Rocky Mountain Oysters 563")
                            {
                                element.Value = "Koothrapaulli Browns 851, Rocky Mountain Oysters 563";
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
            if (string.IsNullOrWhiteSpace(line))
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
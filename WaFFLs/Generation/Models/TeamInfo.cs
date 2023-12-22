using System.Collections.Generic;

namespace WaFFLs.Generation.Models
{
    public class TeamInfo
    {
        public string Filename { get; set; }

        public string Name { get; set; }

        public string Owner { get; set; }

        public List<string> OtherNames { get; set; }

        public int FirstSeason { get; set; }

        public int LastSeason { get; set; }

        public Record SeasonRecord { get; set; }

        public Record PlayoffRecord { get; set; }

        public List<TeamStanding> HeadToHeadRecords { get; set; }

        public List<SeasonInfo> Seasons { get; set; }

        public List<int> ChampionshipYears { get; set; }
    }

    public class SeasonInfo
    {
        public string Name { get; set; }

        public Record Record { get; set; }

        public List<GameInfo> Games { get; set; }
    }

    public class GameInfo
    {
        public string WL => Score > OpponentScore ? "W" : "L";
        public int Score { get; set; }

        public int OpponentScore { get; set; }

        public Team Opponent { get; set; }

        public Week Date { get; set; }
    }

    public class Record
    {
        public int Wins { get; set; }

        public int Losses { get; set; }

        public double Percentage => (double)Wins / (double)(Wins + Losses);
    }

    public class TeamStanding
    {
        public Team Team { get; set; }

        public Record SeasonRecord { get; set; }

        public Record PlayoffRecord { get; set; }
    }
}

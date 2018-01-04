using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace WaFFLs
{
    public class League
    {
        public League()
        {
            Seasons = new List<Season>();
            Teams = new List<Team>();
        }

        public List<Season> Seasons { get; private set; }
        public List<Team> Teams { get; private set; } 
    }

    [DebuggerDisplay("Season {Year}")]
    public class Season
    {
        public Season()
        {
            Weeks = new List<Week>();
            Playoffs = new List<Week>();
        }

        public int Year { get; set; }

        public List<Week> Weeks { get; private set; }
        public List<Week> Playoffs { get; private set; }
    }

    [DebuggerDisplay("Week {Name}")]
    public class Week
    {
        public Week()
        {
            Games = new List<Game>();
        }

        public Season Season { get; set; }

        public string Name { get; set; }
        public List<Game> Games { get; set; } 
    }

    [DebuggerDisplay("{Home.Team.Name} vs {Away.Team.Name}")]
    public class Game
    {
        public Week Week { get; set; }

        public TeamScore Home { get; set; }
        public TeamScore Away { get; set; }
    }

    public static class GameExtensions
    {
        public static TeamScore GetWinningScore(this Game game)
        {
            if (game.Home.Score > game.Away.Score)
            {
                return game.Home;
            }

            if (game.Away.Score > game.Home.Score)
            {
                return game.Away;
            }

            throw new Exception();
        }

        public static TeamScore GetLosingScore(this Game game)
        {
            if (game.Home.Score < game.Away.Score)
            {
                return game.Home;
            }

            if (game.Away.Score < game.Home.Score)
            {
                return game.Away;
            }

            throw new Exception();
        }

        public static Team GetWinningTeam(this Game game)
        {
            return game.GetWinningScore().Team;
        }

        public static Team GetLosingTeam(this Game game)
        {
            return game.GetLosingScore().Team;
        }

        public static Team OpponentOf(this Game game, Team team)
        {
            if (team == game.Home.Team)
            {
                return game.Away.Team;
            }

            if (team == game.Away.Team)
            {
                return game.Home.Team;
            }
            
            throw new Exception();
        }

        public static bool IsWinningTeam(this Game g, Team t)
        {
            if (g.Home.Team == t)
            {
                return g.Home.Score > g.Away.Score;
            }

            if (g.Away.Team == t)
            {
                return g.Away.Score > g.Home.Score;
            }

            throw new Exception();
        }

        public static bool IsLosingTeam(this Game g, Team t)
        {
            if (g.Home.Team == t)
            {
                return g.Home.Score < g.Away.Score;
            }

            if (g.Away.Team == t)
            {
                return g.Away.Score < g.Home.Score;
            }

            throw new Exception();
        }

        public static int GetTeamScore(this Game g, Team t)
        {
            if (g.Home.Team == t)
            {
                return g.Home.Score;
            }

            if (g.Away.Team == t)
            {
                return g.Away.Score;
            }

            throw new Exception();
        }
    }

    [DebuggerDisplay("{Team.Name} {Score}")]
    public class TeamScore
    {
        public Game Game { get; set; }

        public Team Team { get; set; }
        public int Score { get; set; }
    }

    [DebuggerDisplay("{Name}")]
    public class Team
    {
        public Team()
        {
            Games = new List<Game>();
        }

        public string Name { get; set; }

        public List<Game> Games { get; set; }
    }


    public static class ListExtensions
    {
        public static double AverageSafe<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            if (!source.Any())
            {
                return 0.0;
            }

            return source.Average(selector);
        }

        public static double AverageSafe(this IEnumerable<int> source)
        {
            if (!source.Any())
            {
                return 0.0;
            }

            return source.Average();
        }
    }
}

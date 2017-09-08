using System;
using System.Collections.Generic;
using System.Linq;

namespace WaFFLs
{
    public interface ITeamResolver
    {
        Team GetTeamByName(string name);
    }

    public class LeagueTeamResolver : ITeamResolver
    {
        private readonly static Dictionary<string, string> TeamNameCorrections = new Dictionary<string, string>
            {
                { "Anal Cleansing Technician", "Anal Cleansing Technicians" },
                { "Anal Cleansing Techicians", "Anal Cleansing Technicians" },
                { "Blue Tunder", "Blue Thunder" },
                { "Buffy's Bonbardiers", "Buffy's Bombardiers" },
                { "Dont Tase Me Bro", "Don't Tase Me Bro" },
                { "Eraser", "Erasers" },
                { "Marauding Nomds", "Marauding Nomads" },
                { "Maruading Nomads", "Marauding Nomads" },
                { "Nuclear", "Nuclear181" },
                { "Nuclear 181", "Nuclear181" },
                { "Ovepaid Crybabies", "Overpaid Crybabies" },
                { "Phantom", "Phantoms" },
                { "Phantom 122", "Phantom122" },
                { "Rocky Mountain Oystes", "Rocky Mountain Oysters" },
                { "Sporkey's Revenge", "Sporky's Revenge" },
                { "Utracogs", "Ultracogs" },
            };

        private readonly League _league;

        private Dictionary<string, int> counts = new Dictionary<string, int>();

        public LeagueTeamResolver(League league)
        {
            _league = league;
        }

        public Team GetTeamByName(string name)
        {
            if (counts.ContainsKey(name))
            {
                counts[name]++;
            }
            else
            {
                counts.Add(name, 1);
            }

            Team team = _league.Teams.SingleOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
            if (team == null)
            {
                string corrected;
                bool correctionExists = TeamNameCorrections.TryGetValue(name, out corrected);
                if (correctionExists)
                {
                    name = corrected;
                    team = _league.Teams.SingleOrDefault(t => string.Equals(t.Name, corrected, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (team == null)
            {
                team = new Team() { Name = name };
                _league.Teams.Add(team);
            }

            return team;
        }


        internal void ConsoleWriteTeamSpellingsAndCounts()
        {
            var keys = counts.Keys.ToArray();
            Array.Sort(keys);

            foreach (string key in keys)
            {
                Console.WriteLine("{1,-5} - {0}", key, counts[key]);
            }
        }
    }
}
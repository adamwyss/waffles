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
            { "Brawlers", "Brawlers II" },
            { "TeamBean", "Team Bean" },
        };

        private readonly static Dictionary<string, string> TeamNameRenames = new Dictionary<string, string>
        {
            // Ryan Simmons
            { "Avalanche", "Marauding Nomads" }, // 1996-1997
            { "Western Marauders", "Marauding Nomads" }, // 1998
            { "Blue Thunder", "Marauding Nomads" }, // 1999

            // Tad Carlson
            { "Bellevue Renegades", "Sporky's Revenge"}, // 2000

            // Jason Lewis
            { "Washington Whompers", "Just Too Lucky"}, // 2010-2013

            // Brian Wyss
            { "Anal Retentive Pioneers", "Anal Cleansing Technicians"}, // 1996-1998

            // Josh Simmons
            { "Stanwood Stampede", "Phantoms" }, // 1999 (?)
            { "Phantom120", "Phantoms"}, // 2003
            { "Phantom121", "Phantoms"}, // 2004
            { "Phantom122", "Phantoms"}, // 2005

            // Jeannie Thompson
            { "Housh's Your Daddy", "Truffle Shuffle" }, // 2009
        };

        private readonly static Dictionary<string, string> TeamOwners = new Dictionary<string, string>
        {
            { "Marauding Nomads", "Ryan Simmons" },
            { "The TecmoBowlers", "Jason Gadek" },
            { "Team Bean", "Ray Bean" },
            { "Truffle Shuffle", "Jeannie Thompson" },
            { "Low Expectations", "Nathan Clark" },
            { "Speed Demons", "Travis Swingle" },
            { "Fantasy Cognoscenti", "David Machado" },
            { "Phantoms", "Josh Simmons" },
            { "Rocky Mountain Oysters", "Adam Wyss" },
            { "Just Too Lucky", "Jason Lewis" },
            { "Sporky's Revenge", "Tad Carlson" },
            { "Koothrapaulli Browns", "Uday Unni" },
            { "Bayou Boys", "Michael Brister" },
            { "Ultracogs", "Jeff Sucharew" },
            { "Wolves", "Bob Tevis" },
            { "Dominators", "George Demonakos" },
            { "Anal Cleansing Technicians", "Brian Wyss" },
            { "TD Matrix", "Tyler Mellema" },
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

            // this team may have been misspelled accidently.
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

            // the team may have been renamed.  we will use the latest team name.
            string renamed;
            bool renameExists = TeamNameRenames.TryGetValue(name, out renamed);
            if (renameExists)
            {
                name = renamed;
                team = _league.Teams.SingleOrDefault(t => string.Equals(t.Name, renamed, StringComparison.OrdinalIgnoreCase));
            }


            if (team == null)
            {
                team = new Team() { Name = name };
                _league.Teams.Add(team);

                string owner;
                bool hasOwner = TeamOwners.TryGetValue(name, out owner);
                if (hasOwner)
                {
                    team.Owner = owner;
                }

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

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
            { "Blue Thunder", "Marauding Nomads" }, // 1999 (?)

            // Tad Carlson
            { "Bellevue Renegades", "Sporky's Revenge"}, // 2000

            // Jason Lewis
            { "Washington Whompers", "Just Too Lucky"}, // 2010-2013

            // Brian Wyss
            { "Anal Retentive Pioneers", "Anal Cleansing Technicians"}, // 1996-1998

            // Josh Simmons
            { "Slackers", "Phantoms" }, // 1999
            { "Phantom120", "Phantoms"}, // 2003
            { "Phantom121", "Phantoms"}, // 2004
            { "Phantom122", "Phantoms"}, // 2005

            // Jeannie Thompson
            { "Housh's Your Daddy", "Truffle Shuffle" }, // 2009 (?)

            // Eric Franklin
            { "Eternals", "Everett Eternals" }, // 2005-2007

            // Jason Stoner
            { "Screamin' Kookarachas", "M&M Connection" },
            { "Two Thousand Fifty-Three", "M&M Connection" },

            // Tristian Hampton
            { "GSH", "Wishbone" },

            // Carl Diana, Jr.
            { "Lynnwood Parakeets", "Spokanites" },

            // Tyler Mellema
            { "TD Prowler", "TD Matrix" }, // 2009
            // not 100% sure, but TD in the name and tyler's career W/L record is missing the exact amount.  leaving name as td matrix
            // since it was used for 10 years, even though last name used was td prowler
                                           
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
            { "Fighting Calrissians", "Chris Rangel" }, // Theo Fisher for 2015 W8+
            { "Don't Tase Me Bro", "Darren Divito" },
            { "X-Factor", "Theo Fisher" },
            { "Doom Patrol", "Michael Byers" },
            { "Brawlers II", "Justin Bronn" },
            { "A-Team", "Jesse Stoner" },
            { "Red Raiders", "Michael Williams" },
            { "Monica Loves Clinton Dix", "Greg Rockenstire" }, // Anthony Davis for 2018 W9+
            { "Procrastinators", "Richard Scaniffe" },
            { "Overpaid Crybabies", "Michael Elliott" },
            { "Fish On", "Cindy Harris" },
            { "Drunken Squirrels", "Dan Parker" },
            { "Eskimoes", "Ernie Nieto" },
            { "Craig's Broncos", "Craig Myrtle" },
            { "Stormtroopers", "Sean Tindell" },
            { "Nuclear181", "Nickolas Hanson" },
            { "Everett Eternals", "Eric Franklin" },
            { "Erasers", "Matt Charleton" },
            { "Nick Baker's Touchdown Makers", "Nick Baker" },
            { "Buffy's Bombardiers", "Jeff Brister" },
            { "Nonoxynol Nightmares", "David Tyner" }, 
            { "Marshal Law", "Marshal Watson" },
            { "Blitzkrieg", "Jeremy Whitman" },
            { "Anayalaters", "Gustavo Anaya" },
            { "Demons", "Jon Joubert" },
            { "Big Daddy Spanks", "Tim Bunson" },
            { "Twin Bombers", "Doug Pell" },
            { "Kumar's Klan", "Subodh Kumar" },
            { "Oklahoma City Bombers", "Chris Pilon" },
            { "Wolfins", "Chris Crockett & Tom Allanson"},
            { "McBoo", "Larry Boushey & Mike Mickleberry"},
            { "Magic City Rams", "Jay Burroughs"},
            { "Greatest Show on Paper", "Kate Divito" },
            { "No Luck Needed", "Jamie Ham" },
            { "Krypteia", "Patrick Simmons" },
            { "Wishbone", "Tristian Hampton" },
            { "Spokanites", "Carl Diana, Jr." },

            // mostly confident
            { "Spartans", "Tim Miller" },

            // not 100% sure, but the numbers work out
            { "Baldwin on a Budget", "Jake Frauenholtz" },
            { "M&M Connection", "Jason Stoner" },  // 100% confident on M&M, but other teams, less sure.
            { "Stanwood Stampede", "Clint Witherspoon" },
            { "Fourteen Fourteen", "Will Millard" },
            { "Suck My Hawks", "Anthony Davis" }
        };

        private readonly League _league;

        private Dictionary<string, int> counts = new Dictionary<string, int>();

        public LeagueTeamResolver(League league)
        {
            _league = league;
        }

        public static string GetOwner(Team team)
        {
            string owner;
            bool hasOwner = TeamOwners.TryGetValue(team.Name, out owner);
            if (hasOwner)
            {
                return owner;
            }

            return null;
        }

        public static List<string> GetOtherNames(Team team)
        {
            return TeamNameRenames.Where(p => p.Value == team.Name).Select(p => p.Key).Reverse().ToList();
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
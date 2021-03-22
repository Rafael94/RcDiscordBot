using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Services
{
    /// <summary>
    /// Speichert Statis des Channels
    /// </summary>
    internal class OnlineStreamValues
    {
        public OnlineStreamValues(string title, string game)
        {
            Title = title;
            Game = game;
        }

        public string Title { get; set; }
        public string Game { get; set; }

        public override string ToString()
        {
            return $"{Title} - {Game}";
        }

        public override bool Equals(object? obj)
        {
            if (obj is OnlineStreamValues stream)
            {
                return stream.Title == Title && stream.Game == Game;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}

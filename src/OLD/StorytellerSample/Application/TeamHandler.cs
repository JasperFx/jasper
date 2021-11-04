using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;

namespace StorytellerSample.Application
{
    public class TeamHandler
    {
        public static readonly IList<string> Teams = new List<string>();

        public League Handle(TeamAdded added)
        {
            Teams.Fill(added.Name);

            return new League {Teams = Teams.ToArray()};
        }

        public void Handle(GamePlayed played)
        {
            if (played.Home.Score < 0 || played.Visitor.Score < 0)
                throw new InvalidOperationException("Score cannot be negative");
        }

        public void Handle(League league)
        {
            // nothing
        }
    }
}

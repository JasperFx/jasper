using System;
using System.Threading.Tasks;
using Jasper.Messaging.Tracking;
using Jasper.TestSupport.Storyteller;
using StorytellerSample.Application;
using StoryTeller;

namespace StorytellerSample
{
    #region sample_TeamFixture
    public class TeamFixture : MessagingFixture
    {
        [FormatAs("A new team {team} has joined the league")]
        public Task CreateNewTeam(string team)
        {
            // This method sends a message to the service bus and waits
            // until it can detect that the message has been fully processed
            // on the receiving side or timed out
            return Host.SendMessageAndWait(new TeamAdded {Name = team});
        }

        [FormatAs("On {day}, the score was {homeTeam} {homeScore} vs. {visitorTeam} {visitorScore}")]
        public Task RecordGameResult(DateTime day, string homeTeam, int homeScore, string visitorTeam, int visitorScore)
        {
            var message = new GamePlayed
            {
                Date = day.Date,
                Home = new TeamResult {Name = homeTeam, Score = homeScore},
                Visitor = new TeamResult {Name = visitorTeam, Score = visitorScore}
            };

            return Host.SendMessageAndWait(message);
        }

        [FormatAs("Send an un-handled message")]
        public Task SendUnHandledMessage()
        {
            return Host.SendMessageAndWait(new UnhandledMessage());
        }
    }

    #endregion
}

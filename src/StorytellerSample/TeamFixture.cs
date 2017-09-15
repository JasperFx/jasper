using System;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Storyteller;
using StorytellerSample.Application;
using StoryTeller;

namespace StorytellerSample
{
    public class TeamFixture : MessagingFixture
    {
        [FormatAs("A new team {team} has joined the league")]
        public Task CreateNewTeam(string team)
        {
            return SendMessageAndWaitForCompletion(new TeamAdded {Name = team});
        }

        [FormatAs("On {day}, the score was {homeTeam} {homeScore} vs. {visitorTeam} {visitorScore}")]
        public Task RecordGameResult(DateTime day, string homeTeam, int homeScore, string visitorTeam, int visitorScore)
        {
            var message = new GamePlayed
            {
                Date = day.Date,
                Home = new TeamResult{Name = homeTeam, Score = homeScore},
                Visitor = new TeamResult{Name = visitorTeam, Score = visitorScore}
            };

            return SendMessageAndWaitForCompletion(message);
        }

        [FormatAs("Send an un-handled message")]
        public Task SendUnHandledMessage()
        {
            return SendMessageAndWaitForCompletion(new UnhandledMessage());
        }
    }
}

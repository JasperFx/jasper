using System.Linq;
using StoryTeller.Results;
using StoryTeller.Util;

namespace Jasper.Storyteller.Logging
{
    public class MessageHistoryReport : Report
    {
        private readonly EnvelopeHistory[] _envelopes;

        public MessageHistoryReport(EnvelopeRecord[] records)
        {
            _envelopes = records.GroupBy(x => x.Envelope.Id)
                .Select(x => new EnvelopeHistory(x.Key, x))
                .ToArray();
        }


        public string ToHtml()
        {
            var div = new HtmlTag("div");

            div.Add("a").Id("top-of-messages");

            div.Append(toMessageTable());

            div.Add("hr");

            foreach (var envelope in _envelopes)
            {
                var header = div.Add("h4");
                header.Add("a").Id("message-" + envelope.CorrelationId);
                header.Text($"{envelope.MessageType} ({envelope.CorrelationId})");

                div.Append(envelope.ToTableDetails());

                div.Add("a").Text("back to top").Attr("href", "#top-of-messages");
                div.Add("hr");
            }


            return div.ToString();
        }

        public string Title { get; } = "Messages";
        public string ShortTitle { get; } = "Messages";
        public int Count => _envelopes.Length;

        private HtmlTag toMessageTable()
        {
            var table = new TableTag();
            table.AddClass("table").AddClass("table-striped");

            table.AddHeaderRow(row =>
            {
                row.Header("Time").Attr("title",
                    "This is the time in milliseconds since the specification started running that this message was first logged");
                row.Header("Correlation Id");
                row.Header("Message Type");
                row.Header("Details");
            });

            foreach (var history in _envelopes)
                table.AddBodyRow(row =>
                {
                    row.Cell(history.Time.ToString()).Style("text-align", "right");
                    row.Cell(history.CorrelationId.ToString());
                    row.Cell(history.MessageType);
                    row.Add("td/a").Text("details").Attr("href", "#message-" + history.CorrelationId);
                });


            return table;
        }
    }
}

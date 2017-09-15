using System.Collections.Generic;
using System.Linq;
using Baseline;
using StoryTeller.Model;
using StoryTeller.Results;
using StoryTeller.Util;

namespace Jasper.Storyteller.Logging
{
    public class EnvelopeHistory
    {
        public string CorrelationId { get; }

        public EnvelopeHistory(string correlationId, IEnumerable<EnvelopeRecord> records)
        {
            CorrelationId = correlationId;
            Records = records.OrderBy(x => x.Time).ToArray();
        }

        public EnvelopeRecord[] Records { get; set; }
        public long Time => Records[0].Time;
        public string MessageType => Records[0].Envelope.MessageType;

        public HtmlTag ToTableDetails()
        {
            var div = new HtmlTag("div");
            var headers = new TableTag();

            div.Add("h5").Text("Headers");
            div.Append(headers);
            headers.AddClass("table").AddClass("table-striped");
            headers.AddHeaderRow(row =>
            {
                row.Header("Header Key");
                row.Header("Header Value");
            });

            foreach (var pair in Records[0].Envelope.Headers)
            {
                headers.AddBodyRow(row =>
                {
                    row.Cell(pair.Key);
                    row.Cell(pair.Value);
                });
            }

            var table = new TableTag();

            div.Add("h5").Text("Activity");
            div.Append(table);

            table.AddClass("table").AddClass("table-striped");


            table.AddHeaderRow(row =>
            {
                row.Header("Time");
                row.Header("Service");
                row.Header("Message");
            });

            foreach (var record in Records)
            {
                table.AddBodyRow(row =>
                {
                    row.Cell(record.Time.ToString()).Style("text-align", "right");
                    row.Cell(record.ServiceName);
                    row.Cell(record.Message);

                    if (record.ExceptionText.IsNotEmpty())
                    {
                        row.AddClass("bg-warning");
                    }
                });

                if (record.ExceptionText.IsNotEmpty())
                {
                    table.AddBodyRow(row =>
                    {
                        row.Attr("colSpan", "3");
                        row.Add("td/pre").Text(record.ExceptionText);
                    });
                }
            }

            return div;
        }
    }

    public class MessageHistoryReport : Report
    {
        private readonly EnvelopeHistory[] _envelopes;

        public MessageHistoryReport(EnvelopeRecord[] records)
        {
            _envelopes = records.GroupBy(x => x.Envelope.CorrelationId)
                .Select(x => new EnvelopeHistory(x.Key, x))
                .ToArray();
        }

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
            {
                table.AddBodyRow(row =>
                {
                    row.Cell(history.Time.ToString()).Style("text-align", "right");
                    row.Cell(history.CorrelationId);
                    row.Cell(history.MessageType);
                    row.Add("td/a").Text("details").Attr("href", "#message-" + history.CorrelationId);
                });
            }




            return table;
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
    }
}

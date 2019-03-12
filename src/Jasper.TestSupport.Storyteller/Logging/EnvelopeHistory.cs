using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using StoryTeller.Util;

namespace Jasper.TestSupport.Storyteller.Logging
{
    public class EnvelopeHistory
    {
        public EnvelopeHistory(Guid correlationId, IEnumerable<EnvelopeRecord> records)
        {
            CorrelationId = correlationId;
            Records = records.OrderBy(x => x.Time).ToArray();
        }

        public Guid CorrelationId { get; }

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

            var envelope = Records[0].Envelope;
            headers.WriteEnvelopeProperty(envelope, x => x.Source);
            headers.WriteEnvelopeProperty(envelope, x => x.MessageType);
            headers.WriteEnvelopeProperty(envelope, x => x.ReplyUri);
            headers.WriteEnvelopeProperty(envelope, x => x.ContentType);
            headers.WriteEnvelopeProperty(envelope, x => x.CorrelationId);
            headers.WriteEnvelopeProperty(envelope, x => x.CausationId);

            // TODO -- come back and add saga id

            headers.WriteEnvelopeProperty(envelope, x => x.Destination);
            headers.WriteEnvelopeProperty(envelope, x => x.AcceptedContentTypes);
            headers.WriteEnvelopeProperty(envelope, x => x.ReplyRequested);
            headers.WriteEnvelopeProperty(envelope, x => x.AckRequested);
            headers.WriteEnvelopeProperty(envelope, x => x.ExecutionTime);
            headers.WriteEnvelopeProperty(envelope, x => x.DeliverBy);
            headers.WriteEnvelopeProperty(envelope, x => x.ReceivedAt);


            foreach (var pair in envelope.Headers)
                headers.AddBodyRow(row =>
                {
                    row.Cell(pair.Key);
                    row.Cell(pair.Value);
                });

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

                    if (record.ExceptionText.IsNotEmpty()) row.AddClass("bg-warning");
                });

                if (record.ExceptionText.IsNotEmpty())
                    table.AddBodyRow(row =>
                    {
                        row.Attr("colSpan", "3");
                        row.Add("td/pre").Text(record.ExceptionText);
                    });
            }

            return div;
        }
    }
}

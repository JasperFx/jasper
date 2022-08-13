using System;
using System.Collections.Generic;
using Baseline;
using Microsoft.Extensions.Logging;
using StoryTeller.Results;
using StoryTeller.Util;

namespace StorytellerSpecs.Logging;

/// <summary>
///     Used to pipe output from the standard ASP.Net Core ILogger interface
///     in your application to the Storyteller test results
/// </summary>
public class StorytellerAspNetCoreLogger : Report, ILoggerProvider
{
    public StorytellerAspNetCoreLogger(string title = "Logging")
    {
        Title = title;
    }

    internal IList<LogRecord> Records { get; } = new List<LogRecord>();

    void IDisposable.Dispose()
    {
    }

    ILogger ILoggerProvider.CreateLogger(string categoryName)
    {
        return new CategoryLogger(categoryName, this);
    }

    // These 3 properties are for Storytller
    public string Title { get; }

    string Report.ShortTitle => Title;
    int Report.Count => Records.Count;

    // This is the hook that lets you generate raw HTML
    // that will show up as a tab within the results for a spec
    string Report.ToHtml()
    {
        var table = new TableTag();
        table.AddClass("table").AddClass("table-striped");

        table.AddHeaderRow(row =>
        {
            row.Header("Category");
            row.Header("Level");
            row.Header("Message");
        });

        foreach (var record in Records)
        {
            table.AddBodyRow(row =>
            {
                row.Cell(record.Category);
                row.Cell(record.Level);
                row.Cell(record.Message);
            });

            // Write out the full stack trace if there's an exception
            if (record.ExceptionText.IsNotEmpty())
            {
                table.AddBodyRow(row =>
                {
                    row.Cell().Attr("colspan", "3").AddClass("bg-warning").Add("pre").AddClass("bg-warning")
                        .Text(record.ExceptionText);
                });
            }
        }


        return table.ToString();
    }

    internal class LogRecord
    {
        public string Level { get; set; }
        public string Message { get; set; }
        public string ExceptionText { get; set; }
        public string Category { get; set; }
    }

    internal class CategoryLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly StorytellerAspNetCoreLogger _parent;

        public CategoryLogger(string categoryName, StorytellerAspNetCoreLogger parent)
        {
            _categoryName = categoryName;
            _parent = parent;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            var logRecord = new LogRecord
            {
                Category = _categoryName,
                Level = logLevel.ToString(),
                Message = formatter(state, exception),
                ExceptionText = exception?.ToString()
            };

            // Just keep all the log records in an in memory list
            _parent.Records.Add(logRecord);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new Disposable();
        }

        internal class Disposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}

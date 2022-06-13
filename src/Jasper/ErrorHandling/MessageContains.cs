using System;

namespace Jasper.ErrorHandling;

public class MessageContains : IExceptionMatch
{
    private readonly string _text;

    public MessageContains(string text)
    {
        _text = text;
    }

    public string Description => $"Exception message contains \"{_text}\"";
    public Func<Exception, bool> ToFilter() => ex => ex.Message.Contains(_text, StringComparison.OrdinalIgnoreCase);
}

using Microsoft.Extensions.Logging;

namespace StudyRag.Core.Logging;

public static class LogEvents
{
    public static readonly EventId Action = new(1000, nameof(Action));
    public static readonly EventId Response = new(1001, nameof(Response));
}

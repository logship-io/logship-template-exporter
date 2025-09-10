using System;
using System.Collections.Generic;

namespace Logship.Template.Utility
{
    public sealed record LogshipLogEntrySchema(string Schema, DateTime Timestamp, IDictionary<string, object?> Data);
}

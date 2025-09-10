using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Logship.Template.Utility.Internal
{
    internal interface ILogshipExporter
    {
        public Task SendAsync(IReadOnlyList<LogshipLogEntrySchema> entries, CancellationToken token);
    }
}

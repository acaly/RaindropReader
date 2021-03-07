using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaindropReader.Shared.Services.Plugins
{
    internal sealed class ScheduledPluginTask
    {
        public Func<Task> Action { get; init; }
        public TimeSpan Interval { get; init; }

        private DateTime _lastFinishTime;

        public async Task UpdateAsync(DateTime timeUtcCheck)
        {
            if (_lastFinishTime + Interval < timeUtcCheck)
            {
                await Action();
                _lastFinishTime = DateTime.UtcNow;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public interface IScheduledJob
    {
        string JobName { get; }
        TimeSpan Interval { get; }
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}

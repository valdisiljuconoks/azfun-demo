using System.Text;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using Web1.Features.AsciiArt;

namespace Web1.ScheduledJobs
{
    [ScheduledPlugIn(DisplayName = "ScheduledJob1")]
    public class ScheduledJob1 : ScheduledJobBase
    {
        private readonly IAsciiResponseRetriever _retriever;

        public ScheduledJob1(IAsciiResponseRetriever retriever)
        {
            _retriever = retriever;
        }

        public override string Execute()
        {
            OnStatusChanged($"Starting execution of {GetType()}");

            var log = new StringBuilder();

            _retriever.Pump(log);

            return log.ToString();
        }
    }
}

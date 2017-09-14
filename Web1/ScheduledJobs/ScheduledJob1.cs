using System.Configuration;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Shared.Models;

namespace Web1.ScheduledJobs
{
    [ScheduledPlugIn(DisplayName = "ScheduledJob1")]
    public class ScheduledJob1 : ScheduledJobBase
    {
        private bool _stopSignaled;

        public ScheduledJob1()
        {
            IsStoppable = true;
        }

        /// <summary>
        ///     Called when a user clicks on Stop for a manually started job, or when ASP.NET shuts down.
        /// </summary>
        public override void Stop()
        {
            _stopSignaled = true;
        }

        /// <summary>
        ///     Called when a scheduled job executes
        /// </summary>
        /// <returns>A status message to be stored in the database log and visible from admin mode</returns>
        public override string Execute()
        {
            //Call OnStatusChanged to periodically notify progress of job for manually started jobs
            OnStatusChanged($"Starting execution of {GetType()}");

            //Add implementation

            var account = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["MyStorageConnection"].ConnectionString);
            var client = account.CreateCloudQueueClient();
            var queue = client.GetQueueReference("3-done");
            var draftMsg = queue.GetMessage();
            var message = JsonConvert.DeserializeObject<AsciiArtResult>(draftMsg.AsString);

            // do stuff



            queue.DeleteMessage(draftMsg);

            //For long running jobs periodically check if stop is signaled and if so stop execution
            if(_stopSignaled)
            {
                return "Stop of job was called";
            }

            return "Change to message that describes outcome of execution";
        }
    }
}

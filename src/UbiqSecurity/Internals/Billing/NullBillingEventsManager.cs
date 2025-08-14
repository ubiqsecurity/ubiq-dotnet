using System.Threading.Tasks;

namespace UbiqSecurity.Internals.Billing
{
    // A billing events manager that just eats the events and does nothing
    // useful for things like large tests runs, or running in systems w/ no internet access
    internal class NullBillingEventsManager : IBillingEventsManager
    {
        public long EventCount { get; set; }

        public Task AddBillingEventAsync(string apiKey, string datasetName, string datasetGroupName, BillingAction billingAction, DatasetType datasetType, int keyNumber, long count)
        {
            EventCount += count;
            return Task.CompletedTask;
        }

        public void AddUserDefinedMetadata(string jsonString)
        {
        }

        public void Dispose()
        {
        }

        public string GetSerializedEvents() => string.Empty;
    }
}

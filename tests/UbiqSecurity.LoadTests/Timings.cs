using System.Collections.ObjectModel;

namespace UbiqSecurity.LoadTests
{
    public class Timings
    {
        public IDictionary<string, ICollection<double>> EncryptionTimes { get; set; } = new Dictionary<string, ICollection<double>>();

        public ICollection<double> DecryptionTimes { get; set; } = new Collection<double>();

        public int ErrorCount { get; set; } = 0;

        public void AddEncryptionTime(string datasetName, double elapsedMiliseconds)
        {
            if (!EncryptionTimes.ContainsKey(datasetName))
            {
                EncryptionTimes.Add(datasetName, new Collection<double>());
            }

            EncryptionTimes[datasetName].Add(elapsedMiliseconds);
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UbiqSecurity.Internals.WebService;
using UbiqSecurity.Model;

namespace UbiqSecurity.Billing
{
    internal class BillingEventsManager : IBillingEventsManager
	{
		private readonly UbiqConfiguration _configuration;
		private readonly IUbiqWebService _ubiqWebService;
		private readonly ConcurrentDictionary<string, BillingEvent> _billingEvents = new ConcurrentDictionary<string, BillingEvent>();
		private readonly object _lock = new object();

		private DateTime _lastFlushed = DateTime.Now;
		private string _userDefinedMetadata;

		internal BillingEventsManager(UbiqConfiguration configuration, IUbiqWebService ubiqWebService)
		{
			_configuration = configuration;
			_ubiqWebService = ubiqWebService;
		}

		public long EventCount => _billingEvents.Count;

		public async Task AddBillingEventAsync(string apiKey, string datasetName, string datasetGroupName, BillingAction billingAction, DatasetType datasetType, int keyNumber, long count)
		{
			AddBillingEvent(apiKey, datasetName, datasetGroupName, billingAction, datasetType, keyNumber, count);

			await ProcessBillingEventsAsync();
		}

		public void AddUserDefinedMetadata(string jsonString)
		{
			if (jsonString == null)
			{
				throw new ArgumentNullException(nameof(jsonString));
			}

			if (jsonString.Length >= 1024)
			{
				throw new ArgumentException("User defined metadata cannot be longer than 1024 characters", nameof(jsonString));
			}

			if (!jsonString.TryParseJson(out var element))
			{
				throw new ArgumentException("User defined metadata must be a valid Json object", nameof(jsonString));
			}

			_userDefinedMetadata = element.ToString();
		}

		public string GetSerializedEvents()
		{
			var request = new TrackingEventsRequest(_billingEvents.Values.ToArray());

			return JsonConvert.SerializeObject(request);
		}

		public async Task ProcessBillingEventsAsync()
		{
			if (EventCount < _configuration.EventReporting.MinimumCount &&
				DateTime.Now.Subtract(_lastFlushed).TotalSeconds < _configuration.EventReporting.FlushInterval)
			{
				return;
			}

			if (DateTime.Now.Subtract(_lastFlushed).TotalSeconds < _configuration.EventReporting.WakeInterval)
			{
				return;
			}

			await FlushAsync();
		}

		private void AddBillingEvent(string apiKey, string datasetName, string datasetGroupName, BillingAction billingAction, DatasetType datasetType, int keyNumber, long count)
		{
			try
			{
				var billingEvent = new BillingEvent(apiKey, datasetName, datasetGroupName, billingAction, datasetType, keyNumber, count)
				{
					UserDefinedMetadata = _userDefinedMetadata,
					TimestampGranularity = _configuration.EventReporting.ChronoTimestampGranularity,
				};

				var key = billingEvent.ToString();

				_billingEvents.AddOrUpdate(key, billingEvent, (value1, existingBillingEvent) =>
				{
					existingBillingEvent.Count += count;
					existingBillingEvent.LastCalled = DateTime.UtcNow;

					return existingBillingEvent;
				});
			}
			catch
			{
				if (!_configuration.EventReporting.TrapExceptions)
				{
					throw;
				}
			}
		}

		internal async Task FlushAsync()
		{
			if (EventCount == 0)
			{
				return;
			}

			TrackingEventsRequest trackingRequest = null;

			lock(_lock)
			{
				trackingRequest = new TrackingEventsRequest(_billingEvents.Values.ToArray());
				
				_billingEvents.Clear();

				_lastFlushed = DateTime.Now;
			}

			try
			{
				await _ubiqWebService.SendTrackingEventsAsync(trackingRequest).ConfigureAwait(false);
			}
			catch
			{
				if (!_configuration.EventReporting.TrapExceptions)
				{
					throw;
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				FlushAsync().GetAwaiter().GetResult();

				_ubiqWebService?.Dispose();
			}
		}
	}
}

using System;
using System.Collections.Concurrent;
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;
using System.Threading.Tasks;
using UbiqSecurity.Internals;
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

		internal BillingEventsManager(UbiqConfiguration configuration, IUbiqWebService ubiqWebService)
		{
			_configuration = configuration;
			_ubiqWebService = ubiqWebService;
		}

		public int EventCount => _billingEvents.Count;

		public async Task AddBillingEventAsync(string apiKey, string datasetName, string datasetGroupName, BillingAction billingAction, DatasetType datasetType, int keyNumber, long count)
		{
			AddBillingEvent(apiKey, datasetName, datasetGroupName, billingAction, datasetType, keyNumber, count);

			await ProcessBillingEventsAsync();
		}

		public async Task ProcessBillingEventsAsync()
		{
			if (EventCount < _configuration.EventReportingMinimumCount &&
				DateTime.Now.Subtract(_lastFlushed).TotalSeconds < _configuration.EventReportingFlushInterval)
			{
				return;
			}

			if (DateTime.Now.Subtract(_lastFlushed).TotalSeconds < _configuration.EventReportingWakeInterval)
			{
				return;
			}

			await FlushAsync();
		}

		private void AddBillingEvent(string apiKey, string datasetName, string datasetGroupName, BillingAction billingAction, DatasetType datasetType, int keyNumber, long count)
		{
			try
			{
				var billingEvent = new BillingEvent(apiKey, datasetName, datasetGroupName, billingAction, datasetType, keyNumber, count);
				var key = billingEvent.ToString();

				_billingEvents.AddOrUpdate(key, billingEvent, (value1, existingBillingEvent) =>
				{
					existingBillingEvent.Count += count;
					existingBillingEvent.LastCalled = DateTime.UtcNow;

					return existingBillingEvent;
				});
			}
#pragma warning disable CS0168 // Variable is declared but never used
			catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
			{
				if (!_configuration.EventReportingTrapExceptions)
				{
					throw;
				}

#if DEBUG
				Debug.WriteLine($"Trapped exception: {ex.Message}");
#endif
			}

		}

		internal async Task FlushAsync()
		{
			if (EventCount == 0)
			{
				return;
			}

#if DEBUG
			Debug.WriteLine($"Flushing {EventCount} events");
#endif

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
#pragma warning disable CS0168 // Variable is declared but never used
			catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
			{
				if (!_configuration.EventReportingTrapExceptions)
				{
					throw;
				}

#if DEBUG
				Debug.WriteLine($"FlushAsync trapped exception: {ex.Message}");
#endif
			}
		}
	}
}

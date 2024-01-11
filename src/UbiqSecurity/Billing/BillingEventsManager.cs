using System;
using System.Collections.Concurrent;
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
				var billingEvent = new BillingEvent(apiKey, datasetName, datasetGroupName, billingAction, datasetType, keyNumber, count)
				{
					UserDefinedMetadata = _userDefinedMetadata
				};

				var key = billingEvent.ToString();

				_billingEvents.AddOrUpdate(key, billingEvent, (value1, existingBillingEvent) =>
				{
					existingBillingEvent.Count += count;
					existingBillingEvent.LastCalled = DateTime.UtcNow;

					return existingBillingEvent;
				});
			}
#if !DEBUG
#pragma warning disable CS0168 // Variable is declared but never used
#endif
			catch (Exception ex)
#if !DEBUG
#pragma warning restore CS0168 // Variable is declared but never used
#endif
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
			Debug.WriteLine($"Flushing {_billingEvents.Select(x => x.Value.Count).Sum()} events");
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
#if !DEBUG
#pragma warning disable CS0168 // Variable is declared but never used
#endif
			catch (Exception ex)
#if !DEBUG
#pragma warning restore CS0168 // Variable is declared but never used
#endif
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

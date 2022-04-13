using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UbiqSecurity.Model;

namespace UbiqSecurity.Internals
{
	internal class FpeTransactionManager : IDisposable
	{
		#region Private Data

		private List<FpeTransactionRecord> _bills;
		private bool _isFlushing;
		private bool _isProcessingQueueLocked;
		private List<FpeTransactionRecord> _processingQueue;

		private UbiqWebServices _webServices;

		#endregion

		#region Constructors

		internal FpeTransactionManager(UbiqWebServices webServices)
		{
			_bills = new List<FpeTransactionRecord>();
			_isFlushing = false;
			_isProcessingQueueLocked = false;
			_processingQueue = new List<FpeTransactionRecord>();
			_webServices = webServices;
		}

		#endregion

		#region IDisposable Methods

		public void Dispose()
		{
			if(_webServices != null)
			{
				// Perform a final bill  processing for items that may not have been done by the async executor 
				Task.Run(async () =>
				{
					_isFlushing = true;
					_isProcessingQueueLocked = false;
					await ProcessCurrentBillsAsync();
					_webServices.Dispose();
					_webServices = null;
				}).ConfigureAwait(true).GetAwaiter().GetResult();
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Call this whenever a new billable item is created. It adds the transcation
		/// to the _bills item list
		/// </summary>
		/// <param name="id">a unique GUID string</param>
		/// <param name="action">either "encrypt" or "decrypt"</param>
		/// <param name="ffsName">the name of the FFS model, for example "ALPHANUM_SSN"</param>
		/// <param name="timestamp">the timestamp string in ISO8601 format</param>
		/// <param name="count">the number of transactions of this type</param>
		internal void CreateBillableItem (string id, string action, string ffsName, DateTime timestamp, int count)
		{
			var timestampString = timestamp.ToString("o");
			var transaction = new FpeTransactionRecord(id, action, ffsName, timestampString, count);
			_bills.Add(transaction);
		}

		internal async Task ProcessCurrentBillsAsync()
		{
			// take a count of items at this point in time
			// this set the number of items to remove after they have been 
			// moved to the processing queue. So we don't remove any that haven't been 
			// copied over to the processing queue from the billing queue.
			var processingCount = _bills.Count;

			// lock the processing to avoid race conditions
			// billing items keep building while the processingqueue is worked on
			if (!_isProcessingQueueLocked)
			{
				_isProcessingQueueLocked = true;
				try
				{
					// move items to processing queue
					_processingQueue.AddRange(_bills.Take(processingCount));

					if(_processingQueue.Any())
					{
						// only remove the items up to the count that was in the billing list at the start of the process
						_bills.RemoveRange(0, processingCount);

						// minimum 50 items to send to server or system is shutting down
						if (_processingQueue.Count >= 50 || _isFlushing)
						{
							// send items to webservices
							var response = await _webServices.SendBillingAsync(_processingQueue);
							switch (response.Status)
							{
								case 201:
									// remove items from bills
									_processingQueue.Clear();
									break;
								case 400:
									// see which record was last processed
									if (response.LastValidRecord != null)
									{
										var id = response.LastValidRecord.Id;
										var indexOfId = _processingQueue.FindIndex(x => x.Id == id);
										// delete our local list up to and including the last record processed by the backend
										_processingQueue.RemoveRange(0, indexOfId + 1);

										// move the bad record to the end of the billingList 
										// so it won't block the next billing cycle (in case it was a bad record)
										var badItem = _processingQueue.First();
										_processingQueue.RemoveRange(0, 1);
										_bills.Add(badItem);
									}
									break;
								case 504:
									_processingQueue.Clear();
									break;
								default:
									// Go ahead and ignore messages if we get another error.  V3 will handle differently
									_processingQueue.Clear();
									break;
							}
						}
					}
				}
				catch (Exception e)
				{
					throw;
				}
				finally
				{
					_isProcessingQueueLocked = false;
				}
			}
		}

		#endregion
	}
}

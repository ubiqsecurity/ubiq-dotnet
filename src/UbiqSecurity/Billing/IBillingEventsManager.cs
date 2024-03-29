﻿using System;
using System.Threading.Tasks;

namespace UbiqSecurity.Billing
{
	internal interface IBillingEventsManager : IDisposable
	{
		int EventCount { get; }

		Task AddBillingEventAsync(string apiKey, string datasetName, string datasetGroupName, BillingAction billingAction, DatasetType datasetType, int keyNumber, long count);
	}
}

using System;
using System.Threading.Tasks;
using System.Timers;

namespace UbiqSecurity.Internals
{
	internal class FpeProcessor
	{
		private readonly FpeTransactionManager _billManager;
		private Timer _taskTimer;

		internal FpeProcessor(FpeTransactionManager billManager, int secondsToProcess)
		{
			_billManager = billManager;

			double interval = secondsToProcess * 1000;
			_taskTimer = new Timer(interval);
			_taskTimer.Elapsed += OnTimedEvent;
			_taskTimer.AutoReset = true;
		}

		internal void StartUp()
		{
			_taskTimer.Start();
		}

		internal void ShutDown()
		{
			_taskTimer.Stop();
		}

		private void OnTimedEvent(Object source, ElapsedEventArgs e)
		{
			Task.Run(async () => await _billManager.ProcessCurrentBillsAsync());
		}
	}
}

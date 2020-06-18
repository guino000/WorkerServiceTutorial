using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerServiceTutorial
{
    class MonitorLoop
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger _logger;
        private readonly CancellationToken _cancellationToken;

        public MonitorLoop(IBackgroundTaskQueue taskQueue,
            ILogger<MonitorLoop> logger,
            IHostApplicationLifetime applicationLifetime)
        {
            _taskQueue = taskQueue;
            _logger = logger;
            _cancellationToken = applicationLifetime.ApplicationStopping;
        }

        public void StartMonitorLoop()
        {
            _logger.LogInformation("Monitor loop is starting.");

            // Ru a console user input loop in a background thread
            Task.Run(() => Monitor());
        }

        public void Monitor()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                var keystroke = Console.ReadKey();

                if(keystroke.Key == ConsoleKey.W)
                {
                    // Enqueue a background work item
                    _taskQueue.QueueBackgroundWorkItem(async token =>
                    {
                        // Simulate a three 5-second task to complete
                        // for each enqueued work item
                        int delayLoop = 0;
                        var guid = Guid.NewGuid().ToString();

                        _logger.LogInformation(
                            "Queued background task {Guid} is starting.", guid);

                        while (!token.IsCancellationRequested && delayLoop < 3)
                        {
                            try
                            {
                                await Task.Delay(TimeSpan.FromSeconds(5), token);
                            }
                            catch (OperationCanceledException)
                            {
                                // Prevent throwing if the delay is cancelled
                            }

                            delayLoop++;

                            _logger.LogInformation(
                                "Queued background task {Guid} is running. " +
                                "{DelayLoop}/3", guid, delayLoop);
                        }

                        if (delayLoop == 3)
                        {
                            _logger.LogInformation(
                                "Queued background task {Guid} is complete.", guid);
                        }
                        else
                        {
                            _logger.LogInformation(
                                "Queued background task {Guid} was cancelled.", guid);
                        }
                    });
                }
            }
        }
    }
}

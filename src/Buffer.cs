using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Timers;
using System.Threading.Tasks;
using System.Threading;

namespace SMSWorker
{
    public class Buffer<T>
    {
        private readonly Func<T[], Task> _action;
        private readonly int _maxItems;
        private readonly System.Timers.Timer _processBufferTimer;
        private volatile ConcurrentBag<T> _buffer = new ConcurrentBag<T>();

        public Buffer(double intervalInMsecs, int maxItems, Func<T[], Task> action)
        {
            _action = action;
            _maxItems = maxItems;

            _processBufferTimer = new System.Timers.Timer(intervalInMsecs);
            _processBufferTimer.Elapsed += _processBufferTimer_Elapsed;
            _processBufferTimer.Enabled = true;
        }

        private void _processBufferTimer_Elapsed(object sender, ElapsedEventArgs e) => TriggerEvent();

        private void TriggerEvent()
        {
            var readerList = Interlocked.Exchange(ref _buffer, new ConcurrentBag<T>());
            _action?.Invoke(readerList.ToArray());
        }

        public void Add(T item)
        {
            _buffer.Add(item);
            if (_buffer.Count >= _maxItems)
                TriggerEvent();
        }
    }
}

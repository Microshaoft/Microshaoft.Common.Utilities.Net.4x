﻿namespace Microshaoft
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    //private static class QueuedObjectsPoolManager
    //{
    //    public static readonly QueuedObjectsPool<Stopwatch> StopwatchsPool = new QueuedObjectsPool<Stopwatch>(0);
    //}

    public class ConcurrentAsyncQueue<T> :
                                            IPerformanceCountersValuesClearable
    {
        private readonly QueuedObjectsPool<Stopwatch> _stopwatchsPool = null;
        public delegate void QueueEventHandler(T item);
        public event QueueEventHandler OnDequeue;
        public delegate void QueueLogEventHandler(string logMessage);
        public QueueLogEventHandler
                                OnQueueLog
                                , OnDequeueThreadStart
                                , OnDequeueThreadEnd;

        public delegate bool CaughtExceptionEventHandler
                                    (
                                        ConcurrentAsyncQueue<T> sender
                                        , Exception exception
                                        , Exception newException
                                        , string innerExceptionMessage
                                    );
        public event CaughtExceptionEventHandler
                                            OnCaughtException
                                            , OnEnqueueProcessCaughtException
                                            , OnDequeueProcessCaughtException;
        public ConcurrentAsyncQueue(long stopwatchsPoolMaxCapacity = 10 * 10000)
        {
            _stopwatchsPool = new QueuedObjectsPool<Stopwatch>
                                        (
                                            stopwatchsPoolMaxCapacity
                                        );
        }
        private ConcurrentQueue<Tuple<Stopwatch, T>>
                            _queue = new ConcurrentQueue<Tuple<Stopwatch, T>>();

        public ConcurrentQueue<Tuple<Stopwatch, T>> InternalQueue
        {
            get
            {
                return _queue;
            }
            //set { _queue = value; }
        }
        private ConcurrentQueue<Action> _callbackProcessBreaksActions;
        //Microshaoft 用于控制并发线程数
        private long _concurrentDequeueThreadsCount = 0;
        private ConcurrentQueue<ThreadProcessor> _dequeueThreadsProcessorsPool;
        private int _dequeueIdleSleepSeconds = 10;
        public QueuePerformanceCountersContainer PerformanceCountersContainer
        {
            get;
            private set;
        }
        public int DequeueIdleSleepSeconds
        {
            set
            {
                _dequeueIdleSleepSeconds = value;
            }
            get
            {
                return _dequeueIdleSleepSeconds;
            }
        }
        private bool _isAttachedPerformanceCounters = false;
        private readonly object _clearPerformanceCountersValueslocker = new object();

        //public void ClearPerformanceCountersValues(int level)
        //{
        //    //if (this != null)
        //    //ClearPerformanceCountersValues(ref _enabledCountPerformance, level);
        //}
        public void ClearPerformanceCountersValues(int level)
        {
            ////if (this != null)
            lock (_clearPerformanceCountersValueslocker)
            {
                var func = _onEnabledCountPerformanceProcessFunc;
                try
                {
                    _onEnabledCountPerformanceProcessFunc =
                                    (
                                        () =>
                                        {
                                            return false;
                                        }
                                    );
                    var counters = GetPerformanceCountersByLevel(level);
                    foreach (var counter in counters)
                    {
                        counter
                            .RawValue = 0;
                    }
                }
                finally
                {
                    _onEnabledCountPerformanceProcessFunc = func;
                }
            }
        }
        public IEnumerable<PerformanceCounter> PerformanceCounters
        {
            get
            {
                //yield return PerformanceCountersContainer.EnqueuePerformanceCounter;
                //yield return PerformanceCountersContainer.EnqueueRateOfCountsPerSecondPerformanceCounter;
                //yield return PerformanceCountersContainer.QueueLengthPerformanceCounter;
                //yield return PerformanceCountersContainer.DequeuePerformanceCounter;
                //yield return PerformanceCountersContainer.DequeueProcessedRateOfCountsPerSecondPerformanceCounter;
                //yield return PerformanceCountersContainer.DequeueProcessedPerformanceCounter;
                //yield return PerformanceCountersContainer.QueuedWaitAverageTimerPerformanceCounter;
                //yield return PerformanceCountersContainer.DequeueThreadStartPerformanceCounter;
                //yield return PerformanceCountersContainer.DequeueThreadsCountPerformanceCounter;
                //yield return PerformanceCountersContainer.DequeueThreadEndPerformanceCounter;
                //yield return PerformanceCountersContainer.CaughtExceptionsPerformanceCounter;
                return
                    PerformanceCountersContainer
                                .PerformanceCounters;
            }
        }
        public PerformanceCounter GetPerformanceCounterByName(string name)
        {
            return
                PerformanceCountersContainer[name];
        }

        public IEnumerable<PerformanceCounter> GetPerformanceCountersByLevel(int level)
        {
            return
                PerformanceCountersContainer
                    .GetPerformanceCountersByLevel(level);
        }
        private class ThreadProcessor
        {
            public bool Break
            {
                set;
                get;
            }
            public EventWaitHandle Waiter
            {
                private set;
                get;
            }
            public ConcurrentAsyncQueue<T> Sender
            {
                private set;
                get;
            }
            public void StopOne()
            {
                Break = true;
            }
            //public readonly PerformanceCounter[] _incrementCountersBeforeCountPerformanceInThread = null;
            //public readonly PerformanceCounter[] _incrementCountersBeforeCountPerformanceForDequeue = null;
            //public readonly PerformanceCounter[] _decrementCountersBeforeCountPerformanceForDequeue = null;

            //public readonly PerformanceCounter[] _decrementCountersAfterCountPerformanceInThread = null;
            //public readonly PerformanceCounter[] _incrementCountersAfterCountPerformanceInThread = null;
            //public readonly PerformanceCounter[] _incrementCountersAfterCountPerformanceForDequeue = null;

            //public readonly WriteableTuple
            //                        <
            //                            bool
            //                            , Stopwatch
            //                            , PerformanceCounter
            //                            , PerformanceCounter
            //                        >[] _timerCounters = null;

            public ThreadProcessor
                            (
                                ConcurrentAsyncQueue<T> queue
                                , EventWaitHandle wait
                            )
            {
                Waiter = wait;
                Sender = queue;
                QueuePerformanceCountersContainer qpcc = Sender
                                                            .PerformanceCountersContainer;
                //_incrementCountersBeforeCountPerformanceInThread
                //                = new PerformanceCounter[]
                //                        {
                //                            qpcc
                //                                .DequeueThreadStartPerformanceCounter
                //                            , qpcc
                //                                .DequeueThreadsCountPerformanceCounter
                //                        };
                //_decrementCountersAfterCountPerformanceInThread
                //                = new PerformanceCounter[]
                //                        {
                //                                qpcc
                //                                    .DequeueThreadsCountPerformanceCounter
                //                        };
                //_incrementCountersAfterCountPerformanceInThread
                //                = new PerformanceCounter[]
                //                        {
                //                            qpcc
                //                                .DequeueThreadEndPerformanceCounter
                //                        };
                //_incrementCountersBeforeCountPerformanceForDequeue
                //                = new PerformanceCounter[]
                //                        {
                //                            qpcc
                //                                .DequeuePerformanceCounter
                //                        };
                //_decrementCountersBeforeCountPerformanceForDequeue
                //                = new PerformanceCounter[]
                //                        {
                //                            qpcc
                //                                .QueueLengthPerformanceCounter
                //                        };
                //_timerCounters = new WriteableTuple
                //                    <
                //                        bool
                //                        , Stopwatch
                //                        , PerformanceCounter
                //                        , PerformanceCounter
                //                    >[]
                //                {
                //                    WriteableTuple
                //                        .Create
                //                            <
                //                                bool
                //                                , Stopwatch
                //                                , PerformanceCounter
                //                                , PerformanceCounter
                //                            >
                //                            (
                //                                false
                //                                , null
                //                                , qpcc
                //                                    .QueuedWaitAverageTimerPerformanceCounter
                //                                , qpcc
                //                                    .QueuedWaitAverageBasePerformanceCounter
                //                            )
                //                    , WriteableTuple
                //                            .Create
                //                                <
                //                                    bool
                //                                    , Stopwatch
                //                                    , PerformanceCounter
                //                                    , PerformanceCounter
                //                                >
                //                                (
                //                                    true
                //                                    , null
                //                                    , qpcc
                //                                        .DequeueProcessedAverageTimerPerformanceCounter
                //                                    , qpcc
                //                                        .DequeueProcessedAverageBasePerformanceCounter
                //                                )
                //                };
                //_incrementCountersAfterCountPerformanceForDequeue
                //                = new PerformanceCounter[]
                //                                {
                //                                    qpcc
                //                                        .DequeueProcessedPerformanceCounter
                //                                    , qpcc
                //                                        .DequeueProcessedRateOfCountsPerSecondPerformanceCounter
                //                                };
            }
            public void ThreadProcess()
            {
                long l = 0;
                Interlocked.Increment(ref Sender._concurrentDequeueThreadsCount);
                //bool counterEnabled = Sender._isAttachedPerformanceCounters;
                QueuePerformanceCountersContainer qpcc = Sender.PerformanceCountersContainer;
                var queue = Sender.InternalQueue;
                var reThrowException = false;
                
                var enabledCountPerformance = true;
                {
                    if (Sender._onEnabledCountPerformanceProcessFunc != null)
                    {
                        enabledCountPerformance = Sender._onEnabledCountPerformanceProcessFunc();
                    }
                }

                //PerformanceCounter[] incrementCountersBeforeCountPerformanceInThread = null;
                //PerformanceCounter[] decrementCountersAfterCountPerformanceInThread = null;
                //PerformanceCounter[] incrementCountersAfterCountPerformanceInThread = null;

                //if 
                //    (
                //        qpcc != null
                //        &&
                //        enabledCountPerformance
                //    )
                //{
                //    incrementCountersBeforeCountPerformanceInThread
                //                = _incrementCountersBeforeCountPerformanceInThread;
                //    decrementCountersAfterCountPerformanceInThread
                //                = _decrementCountersAfterCountPerformanceInThread;
                //    incrementCountersAfterCountPerformanceInThread
                //                = _incrementCountersAfterCountPerformanceInThread;
                //}

                PerformanceCountersHelper
                        .TryCountPerformance
                            (
                                () =>
                                {
                                    return enabledCountPerformance;
                                }
                                , reThrowException
                                , qpcc.IncrementCountersBeforeCountPerformanceInThread //incrementCountersBeforeCountPerformanceInThread
                                , null
                                , null
                                , () =>
                                {
                                    #region Try Process
                                    if (Sender.OnDequeueThreadStart != null)
                                    {
                                        l = Interlocked.Read(ref Sender._concurrentDequeueThreadsCount);
                                        Sender
                                            .OnDequeueThreadStart
                                                (
                                                    string
                                                        .Format
                                                            (
                                                                "{0} Threads Count {1},Queue Count {2},Current Thread: {3} at {4}"
                                                                , "Threads ++ !"
                                                                , l
                                                                , queue.Count
                                                                , Thread.CurrentThread.Name
                                                                , DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff")
                                                            )
                                                );
                                    }
                                    while (true)
                                    {
                                        #region while true loop
                                        if (Break)
                                        {
                                            break;
                                        }
                                        while (!queue.IsEmpty)
                                        {
                                            #region while queue.IsEmpty loop
                                            if (Break)
                                            {
                                                break;
                                            }
                                            Tuple<Stopwatch, T> item = null;
                                            if (queue.TryDequeue(out item))
                                            {
                                                Stopwatch stopwatchDequeue = Sender
                                                                                ._stopwatchsPool
                                                                                .Get();
                                                Stopwatch stopwatchEnqueue = item.Item1;
                                                //PerformanceCounter[] incrementCountersBeforeCountPerformanceForDequeue = null;
                                                //PerformanceCounter[] decrementCountersBeforeCountPerformanceForDequeue = null;
                                                //PerformanceCounter[] incrementCountersAfterCountPerformanceForDequeue = null;
                                                //WriteableTuple
                                                //    <
                                                //        bool
                                                //        , Stopwatch
                                                //        , PerformanceCounter
                                                //        , PerformanceCounter
                                                //    >[] timerCounters = null;
                                                //if
                                                //    (
                                                //        qpcc != null
                                                //        &&
                                                //        enabledCountPerformance
                                                //    )
                                                //{
                                                //    incrementCountersBeforeCountPerformanceForDequeue =
                                                //            _incrementCountersBeforeCountPerformanceForDequeue;
                                                //    decrementCountersBeforeCountPerformanceForDequeue =
                                                //            _decrementCountersBeforeCountPerformanceForDequeue;
                                                //    _timerCounters[0].Item2 = stopwatchEnqueue;
                                                //    _timerCounters[1].Item2 = stopwatchDequeue;
                                                //    timerCounters = _timerCounters;
                                                //    incrementCountersAfterCountPerformanceForDequeue =
                                                //            _incrementCountersAfterCountPerformanceForDequeue;
                                                   
                                                //}

                                                //var enabledCountPerformance = true;
                                                {
                                                    if (Sender._onEnabledCountPerformanceProcessFunc != null)
                                                    {
                                                        enabledCountPerformance = Sender._onEnabledCountPerformanceProcessFunc();
                                                    }
                                                }
                                                PerformanceCountersHelper
                                                        .TryCountPerformance
                                                            (
                                                                () =>
                                                                {
                                                                    return enabledCountPerformance;
                                                                }
                                                                , reThrowException
                                                                , //incrementCountersBeforeCountPerformanceForDequeue
                                                                    qpcc.IncrementCountersBeforeCountPerformanceForDequeue
                                                                , //decrementCountersBeforeCountPerformanceForDequeue
                                                                    qpcc.DecrementCountersBeforeCountPerformanceForDequeue
                                                                , null //timerCounters
                                                                    //qpcc.TimerCounters
                                                                , () =>			//try
                                                                {
                                                                    if (Sender.OnDequeue != null)
                                                                    {
                                                                        var element = item.Item2;
                                                                        item = null;
                                                                        Sender.OnDequeue(element);
                                                                    }
                                                                }
                                                                , (x, y, z) =>		//catch
                                                                {
                                                                    qpcc
                                                                       .CaughtExceptionsPerformanceCounter
                                                                       .Increment();
                                                                    if (Sender.OnDequeueProcessCaughtException != null)
                                                                    {
                                                                        reThrowException = Sender
                                                                                                .OnDequeueProcessCaughtException
                                                                                                        (
                                                                                                            Sender
                                                                                                            , x
                                                                                                            , y
                                                                                                            , z
                                                                                                        );
                                                                    }
                                                                    if (!reThrowException)
                                                                    {
                                                                        if (Sender.OnCaughtException != null)
                                                                        {
                                                                            reThrowException = Sender.OnCaughtException(Sender, x, y, z);
                                                                        }
                                                                    }
                                                                    return reThrowException;
                                                                }
                                                                , null			//finally
                                                                , null
                                                                , //incrementCountersAfterCountPerformanceForDequeue
                                                                    qpcc.IncrementCountersAfterCountPerformanceForDequeue
                                                            );
                                                //池化
                                                stopwatchEnqueue.Reset();
                                                stopwatchDequeue.Reset();
                                                var r = Sender._stopwatchsPool.Put(stopwatchDequeue);
                                                if (!r)
                                                {
                                                    stopwatchDequeue.Stop();
                                                    stopwatchDequeue = null;
                                                }
                                                r = Sender._stopwatchsPool.Put(stopwatchEnqueue);
                                                if (!r)
                                                {
                                                    stopwatchEnqueue.Stop();
                                                    stopwatchEnqueue = null;
                                                }
                                            }
                                            #endregion while queue.IsEmpty loop
                                        }
                                        #region wait
                                        Sender
                                            ._dequeueThreadsProcessorsPool
                                            .Enqueue(this);
                                        if (Break)
                                        {
                                        }
                                        if (!Waiter.WaitOne(Sender.DequeueIdleSleepSeconds * 1000))
                                        {
                                        }
                                        #endregion wait
                                        #endregion while true loop
                                    }
                                    #endregion
                                }
                                , (x, y, z) =>			//catch
                                {
                                    #region Catch Process
                                    if (Sender.OnCaughtException != null)
                                    {
                                        reThrowException = Sender.OnCaughtException(Sender, x, y, z);
                                    }
                                    return reThrowException;
                                    #endregion
                                }
                                , (x, y, z, w) =>		//finally
                                {
                                    #region Finally Process
                                    l = Interlocked.Decrement(ref Sender._concurrentDequeueThreadsCount);
                                    if (l < 0)
                                    {
                                        Interlocked.Exchange(ref Sender._concurrentDequeueThreadsCount, 0);
                                    }
                                    if (Sender.OnDequeueThreadEnd != null)
                                    {
                                        Sender
                                            .OnDequeueThreadEnd
                                                (
                                                    string.Format
                                                            (
                                                                "{0} Threads Count {1},Queue Count {2},Current Thread: {3} at {4}"
                                                                , "Threads--"
                                                                , l
                                                                , Sender
                                                                        .InternalQueue
                                                                        .Count
                                                                , Thread
                                                                        .CurrentThread
                                                                        .Name
                                                                , DateTime
                                                                        .Now
                                                                        .ToString("yyyy-MM-dd HH:mm:ss.fffff")
                                                            )
                                                );
                                    }
                                    if (!Break)
                                    {
                                        Sender
                                            .StartIncreaseDequeueProcessThreads(1);
                                    }
                                    Break = false;
                                    #endregion
                                }
                                , //decrementCountersAfterCountPerformanceInThread
                                    qpcc.DecrementCountersAfterCountPerformanceInThread
                                , //incrementCountersAfterCountPerformanceInThread
                                    qpcc.IncrementCountersAfterCountPerformanceInThread
                            );
            }
        }
        public void AttachPerformanceCounters
                        (
                            string instanceNamePrefix
                            , string categoryName
                            , QueuePerformanceCountersContainer performanceCounters
                            , Func<bool> onEnabledCountPerformanceProcessFunc = null
                            , PerformanceCounterInstanceLifetime
                                        performanceCounterInstanceLifetime
                                            = PerformanceCounterInstanceLifetime.Global
                            , long? initializePerformanceCounterInstanceRawValue = null
                        )
        {
            var process = Process.GetCurrentProcess();
            var processName = process.ProcessName;
            var instanceName = string
                                    .Format
                                        (
                                            "{0}-{1}"
                                            , instanceNamePrefix
                                            , processName
                                        );
            PerformanceCountersContainer = performanceCounters;
            PerformanceCountersContainer
                .AttachPerformanceCountersToMembers
                        (
                            categoryName
                            , instanceName
                            , performanceCounterInstanceLifetime
                            , initializePerformanceCounterInstanceRawValue
                        );
            PerformanceCountersContainer
                .RegisterCountersUsage();
            _isAttachedPerformanceCounters = true;
            _onEnabledCountPerformanceProcessFunc = onEnabledCountPerformanceProcessFunc;
        }
        public int Count
        {
            get
            {
                return _queue.Count;
            }
        }
        public long ConcurrentThreadsCount
        {
            get
            {
                return _concurrentDequeueThreadsCount;
            }
        }
        public QueuedObjectsPool<Stopwatch> StopwatchsPool
        {
            get
            {
                return _stopwatchsPool;
            }
        }
        private void DecreaseDequeueProcessThreads(int count)
        {
            Action action;
            for (var i = 0; i < count; i++)
            {
                if (_callbackProcessBreaksActions.TryDequeue(out action))
                {
                    action();
                    action = null;
                }
            }
        }
        public void StartDecreaseDequeueProcessThreads(int count)
        {
            new Thread
                    (
                        new ThreadStart
                                (
                                    () =>
                                    {
                                        DecreaseDequeueProcessThreads(count);
                                    }
                                )
                    ).Start();
        }
        public void StartIncreaseDequeueProcessThreads(int count)
        {
            new Thread
                    (
                        new ThreadStart
                                (
                                    () =>
                                    {
                                        IncreaseDequeueProcessThreads(count);
                                    }
                                )
                    ).Start();
        }
        private void IncreaseDequeueProcessThreads(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Interlocked.Increment(ref _concurrentDequeueThreadsCount);
                if (_dequeueThreadsProcessorsPool == null)
                {
                    _dequeueThreadsProcessorsPool = new ConcurrentQueue<ThreadProcessor>();
                }
                var processor = new ThreadProcessor
                                                (
                                                    this
                                                    , new AutoResetEvent(false)
                                                );
                var thread = new Thread
                                    (
                                        new ThreadStart
                                                    (
                                                        processor.ThreadProcess
                                                    )
                                    );
                if (_callbackProcessBreaksActions == null)
                {
                    _callbackProcessBreaksActions = new ConcurrentQueue<Action>();
                }
                var callbackProcessBreakAction = new Action
                                                        (
                                                            processor.StopOne
                                                        );
                _callbackProcessBreaksActions.Enqueue(callbackProcessBreakAction);
                _dequeueThreadsProcessorsPool.Enqueue(processor);
                thread.Start();
            }
        }
        //private bool _enabledCountPerformance = false;

        //public bool EnabledCountPerformance
        //{
        //    get { return _enabledCountPerformance; }
        //    set { _enabledCountPerformance = value; }
        //}

        //private bool _enabledCountPerformance = false;

        private Func<bool> _onEnabledCountPerformanceProcessFunc;
        //{
        //    get;
        //    set;
        //}


        public bool Enqueue(T item)
        {
            var r = false;
            var reThrowException = false;
            //var enableCount = _isAttachedPerformanceCounters;

            var enabledCountPerformance = true;
            if (_onEnabledCountPerformanceProcessFunc != null)
            {
                enabledCountPerformance = _onEnabledCountPerformanceProcessFunc();
            }
            var qpcc = PerformanceCountersContainer;
            PerformanceCountersHelper
                .TryCountPerformance
                    (
                        () =>
                        {
                            return enabledCountPerformance;
                        }
                        , reThrowException
                        , qpcc
                            .IncrementCountersBeforeCountPerformanceForEnqueue
                           //incrementCountersBeforeCountPerformance
                        , null
                        , null
                        , () =>
                        {
                            Stopwatch stopwatchEnqueue = null;
                            if (_isAttachedPerformanceCounters)
                            {
                                stopwatchEnqueue = _stopwatchsPool.Get();
                            }
                            stopwatchEnqueue.Start();
                            var element = Tuple
                                            .Create
                                                (
                                                    stopwatchEnqueue
                                                    , item
                                                );
                            _queue.Enqueue(element);
                            r = true;
                        }
                        , (x, y, z) =>
                        {
                            qpcc
                                .CaughtExceptionsPerformanceCounter
                                .Increment();
                            if (OnEnqueueProcessCaughtException != null)
                            {
                                reThrowException = OnEnqueueProcessCaughtException(this, x, y, z);
                            }
                            if (!reThrowException)
                            {
                                if (OnCaughtException != null)
                                {
                                    reThrowException = OnCaughtException(this, x, y, z);
                                }
                            }
                            return reThrowException;
                        }
                        , (x, y, z, w) =>
                        {
                            if
                                (
                                    _dequeueThreadsProcessorsPool != null
                                    && !_dequeueThreadsProcessorsPool.IsEmpty
                                )
                            {
                                ThreadProcessor processor;
                                if 
                                    (
                                        _dequeueThreadsProcessorsPool
                                                .TryDequeue(out processor)
                                    )
                                {
                                    processor.Waiter.Set();
                                    processor = null;
                                    //Console.WriteLine("processor = null;");
                                }
                            }
                        }
                    );
            return r;
        }
    }
}


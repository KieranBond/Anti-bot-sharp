﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AntiBotSharp.Helpers
{
    public class Timewatch
    {
        public static Timewatch Instance { get { return _instance.Value; } }
        private static readonly Lazy<Timewatch> _instance = new Lazy<Timewatch>(true);

        private Stopwatch _stopWatch;
        private DateTime _lastTimeTick;

        private Dictionary<string, Timer> _timers;
        private readonly object _timerLock = new object();

        public Timewatch()
        {
            Initialize();
        }

        private void Initialize()
        {
            _timers = new Dictionary<string, Timer>();
            _lastTimeTick = DateTime.Now;
            _stopWatch = new Stopwatch();
            Task.Run(BeginTicking);
        }

        private void BeginTicking()
        {
            _stopWatch.Start();
            do
            {
                double deltaTime = _stopWatch.ElapsedMilliseconds * 0.001d;//Convert to seconds
                _stopWatch.Restart();

                if (_timers.Count <= 0)
                {
                    _lastTimeTick = DateTime.Now;
                    continue;
                }

                lock (_timerLock)
                {
                    List<string> toRemove = new List<string>();
                    foreach (string key in _timers.Keys)
                    {
                        if (_timers[key].CanDestroy)
                            toRemove.Add(key);
                    }

                    foreach (string key in toRemove)
                        _timers.Remove(key);

                    foreach (Timer timer in _timers.Values)
                    {
                        timer.IncrementTimer(deltaTime);
                    }
                }

                _lastTimeTick = DateTime.Now;

                Thread.Sleep(100);
            }
            while (true);

        }

        public static string AddTimer(double durationInSeconds, Action callback)
        {
            return Instance.AddCallbackTimer(durationInSeconds, callback);
        }

        public static void RemoveTimer(string id)
        {
            Instance.RemoveCallbackTimer(id);
        }

        public static int GetRemainingTime(string timerID)
        {
            double duration = -1d;
 
            if (Instance._timers.Count <= 0)
                return (int)duration;

            lock(Instance._timerLock)
            {
                if(Instance._timers.ContainsKey(timerID))
                    duration = Instance._timers[timerID].TimeRemainingInSeconds;
            }

            return (int)duration;
        }

        private string AddCallbackTimer(double durationInSeconds, Action callback)
        {
            var timer = new Timer(durationInSeconds, callback);
            var id = Guid.NewGuid().ToString();
            lock (_timerLock)
            {
                _timers.Add(id, timer);
            }

            return id;
        }

        private void RemoveCallbackTimer(string id)
        {
            if (_timers.Count > 0 && _timers.ContainsKey(id))
            {
                lock (_timerLock)
                {
                    if(_timers.ContainsKey(id))
                        _timers.Remove(id);
                }
            }
        }
    }

    public class Timer
    {
        public bool CanDestroy { get; private set; }

        public double TimeRemainingInSeconds { get; private set; }
        private Action _callback;

        public Timer(double durationInSeconds, Action callback)
        {
            TimeRemainingInSeconds = durationInSeconds;
            _callback = callback;
        }

        public void IncrementTimer(double timeIncrementInSeconds)
        {
            TimeRemainingInSeconds -= timeIncrementInSeconds;

            if (TimeRemainingInSeconds <= 0d && !CanDestroy)
            {
                _callback.Invoke();
                CanDestroy = true;
            }
        }
    }
}

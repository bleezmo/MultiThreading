using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreading
{
    public abstract class IncrementBase : IRunnable
    {
        public static int Threads = 20;
        public static int MaxLoops = 2000000;
        protected static int count = 0;
        public async Task Run()
        {
            var tasks = new List<Task>();
            for (var i = 0; i < Threads; i++)
            {
                tasks.Add(Loop(MaxLoops));
            }
            var s = Stopwatch.StartNew();
            await Task.WhenAll(tasks);
            s.Stop();
            Console.WriteLine($"Completed. count = {count}. Time: {s.ElapsedMilliseconds}");
            count = 0;
        }
        private async Task Loop(int max)
        {
            await Task.Yield();
            for (int i = 0; i < max; i++)
            {
                Increment();
            }
        }
        protected abstract void Increment();
    }
    public class IncrementSeparated : IncrementBase
    {
        protected override void Increment()
        {
            int current = count;
            current++;
            count = current;
        }
    }
    public class IncrementRegular : IncrementBase
    {
        protected override void Increment()
        {
            count++;
        }
    }
    public class LockIncrement : IncrementBase
    {
        private static object _lock = new object();
        protected override void Increment()
        {
            lock (_lock)
            {
                count++;
            }
        }
    }
    public class AtomicIncrement : IncrementBase
    {
        protected override void Increment()
        {
            Interlocked.Increment(ref count);
        }
    }
    public class SemaphoreIncrement : IncrementBase
    {
        private static SemaphoreSlim _sem = new SemaphoreSlim(1,1);
        protected override void Increment()
        {
            _sem.Wait();
            count++;
            _sem.Release();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreading
{
    public class SemVsMutex : IRunnable
    {
        public async Task Run()
        {
            var sem = new SemaphoreLock();
            var mut = new MutexLock();
            var tasks = new List<Task>();
            Console.WriteLine("Running blocking lock function...");
            for (var i = 0; i < 3; i++)
            {
                tasks.Add(Task.Run(() => RunLock(sem)));
                tasks.Add(Task.Run(() => RunLock(mut)));
            }
            await Task.WhenAll(tasks);
            tasks = new List<Task>();
            Console.WriteLine("Running non-blocking lock function...");
            for (var i = 0; i < 3; i++)
            {
                tasks.Add(RunLockAsync(sem));
                tasks.Add(RunLockAsync(mut));
            }
            await Task.WhenAll(tasks);
        }
        async Task RunLockAsync(LockMechanism lm)
        {
            try
            {
                Console.WriteLine($"Running {lm.GetType().Name}");
                lm.Wait();
                await Task.Delay(1000);
                lm.Release();
            }catch(Exception e)
            {
                Console.WriteLine($"Error in {nameof(RunLockAsync)} using {lm.GetType().Name}: {e.ToString()}");
            }
        }
        void RunLock(LockMechanism lm)
        {
            try
            {
                Console.WriteLine($"Running {lm.GetType().Name}");
                lm.Wait();
                Thread.Sleep(1000);
                lm.Release();
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error in {nameof(RunLock)} using {lm.GetType().Name}: {e.ToString()}");
            }
        }
        interface LockMechanism : IDisposable
        {
            void Wait();
            void Release();
        }
        class SemaphoreLock : LockMechanism
        {
            private readonly SemaphoreSlim _sem = new SemaphoreSlim(1, 1);

            public void Wait()
            {
                _sem.Wait();
            }
            public void Release()
            {
                _sem.Release();
            }
            public void Dispose()
            {
                _sem.Dispose();
            }
        }
        class MutexLock : LockMechanism
        {
            private readonly Mutex _mut = new Mutex();

            public void Wait()
            {
                _mut.WaitOne();
            }
            public void Release()
            {
                _mut.ReleaseMutex();
            }
            public void Dispose()
            {
                _mut.Dispose();
            }
        }
    }
}

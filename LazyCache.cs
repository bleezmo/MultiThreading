using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreading
{
    public class LazyCache : IRunnable
    {
        private static readonly MemoryCache _cacheManager = new MemoryCache("meh");
        private static readonly ConcurrentDictionary<string, Task<string>> _lazyDictionary = new ConcurrentDictionary<string, Task<string>>();

        public async Task Run()
        {
            Console.WriteLine("Calling Cache demo 1");
            var tasks = new List<Task>();
            for(var i = 0; i < 5; i++)
            {
                tasks.Add(CacheIOCall1("hello"));
            }
            await Task.WhenAll(tasks); 
            tasks = new List<Task>();
            for (var i = 0; i < 5; i++)
            {
                tasks.Add(CacheIOCall1("hello"));
            }
            await Task.WhenAll(tasks);
            _cacheManager.Remove("hello");
            Console.WriteLine("Calling Cache demo 2");
            tasks = new List<Task>();
            for (var i = 0; i < 5; i++)
            {
                tasks.Add(CacheIOCall2("hello"));
            }
            await Task.WhenAll(tasks);
            tasks = new List<Task>();
            for (var i = 0; i < 5; i++)
            {
                tasks.Add(CacheIOCall2("hello"));
            }
            await Task.WhenAll(tasks);
        }
        /// <summary>
        /// bad because multiple calls can occur in I/O
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private async Task<string> CacheIOCall1(string key)
        {
            var value = _cacheManager.Get(key) as string;
            if (value != null)
            {
                return value;
            }
            value = await ExpensiveIOCall(key);
            _cacheManager.AddOrGetExisting(key, value, DateTimeOffset.Now.AddMinutes(10));
            return value;
        }
        /// <summary>
        /// good
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private async Task<string> CacheIOCall2(string key)
        {
            var value = _cacheManager.Get(key) as string;
            if (value != null)
            {
                return value;
            }
            return await _lazyDictionary.GetOrAdd(key, async k => {
                value = await ExpensiveIOCall(k);
                _cacheManager.AddOrGetExisting(key, value, DateTimeOffset.Now.AddMinutes(10));
                Timer t = null;
                t = new Timer(dc => {
                    _lazyDictionary.Remove(key, out _);
                    t.Dispose();
                }, null, 1000, Timeout.Infinite);
                return value;
            });
        }
        private static async Task<string> ExpensiveIOCall(string key)
        {
            Console.WriteLine("Calling expensive IO call...");
            await Task.Delay(5000);
            if (key == "hello") return "world";
            else if (key == "blargl") return "margl";
            else if (key == "meh") return "merp";
            else return null;
        }
    }
}

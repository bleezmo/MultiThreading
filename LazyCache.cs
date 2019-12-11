using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreading
{
    public class LazyCache : IRunnable
    {
        private static readonly ConcurrentDictionary<string, string> _dictionary = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<string, Task<string>> _lazyDictionary = new ConcurrentDictionary<string, Task<string>>();
        private static readonly SemaphoreSlim _sem = new SemaphoreSlim(1, 1);

        public async Task Run()
        {
            var tasks = new List<Task>();
            for(var i = 0; i < 10; i++)
            {
                tasks.Add(CacheIOCall3("hello"));
            }
            await Task.WhenAll(tasks);
            tasks = new List<Task>();
            for (var i = 0; i < 5; i++)
            {
                tasks.Add(CacheIOCall3("hello"));
            }
            await Task.WhenAll(tasks);
        }
        private async Task<string> CacheIOCall1(string key)
        {
            if(_dictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            value = await ExpensiveIOCall(key);
            _dictionary[key] = value;
            return value;
        }
        private async Task<string> CacheIOCall2(string key)
        {
            if (_dictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            await _sem.WaitAsync();
            try
            {
                value = await _lazyDictionary.GetOrAdd(key, k => ExpensiveIOCall(k));
                return _dictionary.GetOrAdd(key, k =>
                {
                    _lazyDictionary.Remove(key, out _);
                    return value;
                });
            }
            finally
            {
                _sem.Release();
            }
        }
        private async Task<string> CacheIOCall3(string key)
        {
            if (_dictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            await _sem.WaitAsync();
            try
            {
                return await _lazyDictionary.GetOrAdd(key, k => {
                    return Task.Run(async () =>
                    {
                        if(_dictionary.TryGetValue(k, out value))
                        {
                            _lazyDictionary.TryRemove(k, out _);
                            return value;
                        }
                        var result = await ExpensiveIOCall(k);
                        return _dictionary.GetOrAdd(k, k1 =>
                        {
                            _lazyDictionary.TryRemove(k1, out _);
                            return result;
                        });
                    });
                });
            }
            finally
            {
                _sem.Release();
            }
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

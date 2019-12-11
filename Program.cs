using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiThreading
{
    class Program
    {
        static void Main(string[] args)
        {
            Run(args);
        }
        static void Run(string[] args)
        {
            var runAll = args.Length == 0;
            var type = typeof(IRunnable);
            var runnables = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract && (runAll || args.Any(a => p.Name == a)))
                .Select(p => new KeyValuePair<string, IRunnable>(p.Name, Activator.CreateInstance(p) as IRunnable));
            foreach (var runnable in runnables)
            {
                Console.WriteLine($"Running {runnable.Key}");
                runnable.Value.Run().Wait();
            }
        }
    }
}

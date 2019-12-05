using System;

namespace MultiThreading
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach(var arg in args){
                Activator.CreateInstance(AppDomain.CurrentDomain, args[1], null);
                var type = typeof(IRunnable);
                var runnable = Activator.CreateInstance(AppDomain.CurrentDomain.GetAssemblies()
                                    .SelectMany(s => s.GetTypes())
                                    .FirstOrDefault(p => type.IsAssignableFrom(p) && p.Name == arg)) as IRunnable;
                runnable.Run();
            }
            Console.WriteLine("Hello Worlds!");
        }
    }
}

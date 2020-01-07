using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreading
{
    public abstract class ScopeBase : IRunnable
    {
        protected static int count = 0;
        protected const int maxLoops = 5;
        private Random _rnd = new Random();
        public async Task Run()
        {
            var tasks = new Task<string>[]
            {
                Concat("A"),
                Concat("B"),
                Concat("C")
            };
            foreach(var result in await Task.WhenAll(tasks))
            {
                Console.WriteLine(result);
            }
        }
        protected virtual async Task DoStuff()
        {
            await Task.Delay(_rnd.Next(100, 500));
        }
        protected abstract Task<string> Concat(string prefix, int count = 0);
    }
    public class AsyncScope : ScopeBase
    {
        static readonly AsyncLocal<Scope> _scope = new AsyncLocal<Scope>();

        protected override async Task<string> Concat(string prefix, int count = 0)
        {
            _scope.Value = new Scope(_scope.Value, $"{prefix}{count}");
            await DoStuff();
            var msg = _scope.Value.Message;
            _scope.Value = _scope.Value.Parent;
            return count < maxLoops ? $"{msg} => {await Concat(prefix, count + 1)}" : msg;
        }
    }
    public class NonAsyncScope : ScopeBase
    {
        static Scope _scope;

        protected override async Task<string> Concat(string prefix, int count = 0)
        {
            _scope = new Scope(_scope, $"{prefix}{count}");
            await DoStuff();
            if(_scope != null) //sometimes scope is set to null from other threads
            {
                var msg = _scope.Message;
                _scope = _scope.Parent;
                return count < maxLoops ? $"{msg} => {await Concat(prefix, count + 1)}" : msg;
            }
            return string.Empty;
        }
    }
    class Scope
    {
        public Scope(string msg)
        {
            Message = msg;
        }
        public Scope(Scope parent, string msg)
        {
            Parent = parent;
            Message = msg;
        }
        public virtual Scope Parent { get; set; }
        public virtual string Message { get; set; }
    }
}

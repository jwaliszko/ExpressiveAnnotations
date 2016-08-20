using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Castle.Core.Internal;
using ExpressiveAnnotations.MvcUnobtrusive.Caching;
using Xunit;

namespace ExpressiveAnnotations.MvcUnobtrusive.Tests
{
    public class MapCacheTest
    {
        public MapCacheTest()
        {
            MapCache<string, TestItem>.Clear();
        }

        [Fact]
        public void verify_bahaviour_for_concurrent_access_under_different_keys()
        {
            var keys = new[] {"a", "b"};
            var counter = new ConcurrentStack<int>(); // value factory threads
            var storage = new ConcurrentStack<TestItem>(); // cached items

            // first run
            var threads = MakeThreads(keys);
            threads.ForEach(t => t.Start(new object[] {storage, counter}));
            threads.ForEach(t => t.Join());

            Assert.Equal(2, counter.Count);
            Assert.Equal(2, storage.Count);
            Assert.NotSame(storage.First(), storage.Last());
            var a = storage.FirstOrDefault(x => x.Id == "a");
            var b = storage.FirstOrDefault(x => x.Id == "b");

            // cleanups and second run
            storage.Clear();
            counter.Clear();

            threads = MakeThreads(keys);
            threads.ForEach(t => t.Start(new object[] {storage, counter}));
            threads.ForEach(t => t.Join());

            Assert.Equal(0, counter.Count);
            Assert.Equal(2, storage.Count);
            Assert.NotSame(storage.First(), storage.Last());
            var aa = storage.FirstOrDefault(x => x.Id == "a");
            var bb = storage.FirstOrDefault(x => x.Id == "b");
            Assert.Same(a, aa);
            Assert.Same(b, bb);
        }

        [Fact]
        public void verify_bahaviour_for_concurrent_access_under_identical_keys()
        {
            var keys = new[] {"a", "a"};
            var counter = new ConcurrentStack<int>();
            var storage = new ConcurrentStack<TestItem>();

            // first run
            var threads = MakeThreads(keys);
            threads.ForEach(t => t.Start(new object[] {storage, counter}));
            threads.ForEach(t => t.Join());

            Assert.Equal(1, counter.Count);
            Assert.Equal(2, storage.Count);
            var a = storage.First();
            Assert.Same(storage.First(), storage.Last());
            
            // cleanups and second run
            storage.Clear();
            counter.Clear();

            threads = MakeThreads(keys);
            threads.ForEach(t => t.Start(new object[] {storage, counter}));
            threads.ForEach(t => t.Join());

            Assert.Equal(0, counter.Count);
            Assert.Equal(2, storage.Count);
            var aa = storage.First();
            Assert.Same(storage.First(), storage.Last());
            Assert.Same(a, aa);
        }

        private Thread[] MakeThreads(string[] keys)
        {
            var threads = keys.Select(key =>
                new Thread(load =>
                {
                    var storage = (ConcurrentStack<TestItem>) ((object[]) load)[0];
                    var counter = (ConcurrentStack<int>) ((object[]) load)[1];

                    var item = MapCache<string, TestItem>.GetOrAdd(key.ToString(), _ =>
                    {
                        Debug.WriteLine($"{key} :: {Thread.CurrentThread.ManagedThreadId}");
                        counter.Push(Thread.CurrentThread.ManagedThreadId); // we want to test that this value factory delegate is invoked only once, even if map is accessed concurrently for the same key
                        Thread.Sleep(500);
                        return new TestItem {Id = key};
                    });
                    storage.Push(item);
                })).ToArray();
            return threads;
        }

        private class TestItem
        {
            public string Id { get; set; }
        }
    }
}

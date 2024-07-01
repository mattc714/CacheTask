using Newtonsoft.Json.Linq;
using CacheTask;

namespace CacheTests.Tests
{
    public class CacheThreadSafetyTests
    {  
        [SetUp]
        public void Setup()
        {
            Cache.Instance.SetMaxCacheItems(5);
            Cache.Instance.Clear(); // remove all items from cache before each test
        }



        [TestCase(1)]
        [TestCase(100)]
        [TestCase(int.MaxValue)]
        public void SetMaxCacheItems_ValidValue_UpdatesCorrectly(int capacity)
        {
            Cache.Instance.SetMaxCacheItems(capacity);
            Assert.That(Cache.Instance.MaxCacheItems, Is.EqualTo(capacity));
        }


        [Test]
        public void AddOrUpdate_ThreadSafety()
        {
            Parallel.For(0, 1000, i =>
            {
                Cache.Instance.AddOrUpdate($"key{i}", $"value{i}");
            });

            Assert.That(Cache.Instance.Length, Is.EqualTo(5)); // Only 5 items should be in the cache
        }

        [Test]
        public void Get_ThreadSafety()
        {
            Cache.Instance.AddOrUpdate("key1", "value1");
            Parallel.For(0, 1000, i =>
            { 
                Assert.That(Cache.Instance.Get("key1"), Is.EqualTo("value1"));
            });
        }

        [Test]
        public void Remove_ThreadSafety()
        {
            Cache.Instance.AddOrUpdate("key1", "value1");
            Parallel.For(0, 1000, i =>
            {
                Cache.Instance.Remove("key1");
            });

            Assert.IsNull(Cache.Instance.Get("key1"));
        }

        [Test]
        public void Clear_ThreadSafety()
        {
            Parallel.For(0, 1000, i =>
            {
                Cache.Instance.Clear();
            });

            Assert.That(Cache.Instance.Length, Is.EqualTo(0));
        }

    }
}
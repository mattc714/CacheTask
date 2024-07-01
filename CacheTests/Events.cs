using Newtonsoft.Json.Linq;
using CacheTask;

namespace CacheTests.Tests
{
    public class CacheEventTests
    {
        [SetUp]
        public void Setup()
        {
            Cache.Instance.SetMaxCacheItems(5);
            Cache.Instance.Clear(); // remove all items from cache before each test
        }

        [Test]
        public void CacheItemEvictionEvent_IsTriggered()
        {
            string evictedKey = null;
            Cache.Instance.OnItemEvicted += (sender, key) => evictedKey = key;

            Cache.Instance.AddOrUpdate("key1", "value1");
            Cache.Instance.AddOrUpdate("key2", "value2");
            Cache.Instance.AddOrUpdate("key3", "value3");
            Cache.Instance.AddOrUpdate("key4", "value4");
            Cache.Instance.AddOrUpdate("key5", "value5");
            Cache.Instance.AddOrUpdate("key6", "value6"); // Should evict key1

            Assert.That(evictedKey, Is.EqualTo("key1"));
        }
    }
}
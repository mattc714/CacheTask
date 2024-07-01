using Newtonsoft.Json.Linq;
using CacheTask;

namespace CacheTests.Tests
{
    public class CacheCapactityTests
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
        public void SetMaxCacheItems_ExistingCacheItems_RemovesLeastAccessed()
        {
            for (int i = 1; i <= 5; i++)
                Cache.Instance.AddOrUpdate("key" + i, "value" + i);

            Assert.That(Cache.Instance.Length, Is.EqualTo(5));

            Cache.Instance.SetMaxCacheItems(1);
            Assert.That(Cache.Instance.MaxCacheItems, Is.EqualTo(1));
            Assert.That(Cache.Instance.Length, Is.EqualTo(1));
            Assert.That(Cache.Instance.Get("key5"), Is.EqualTo("value5"));
        }


        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-int.MaxValue)]
        public void SetMaxCacheItems_NegativeValue_ThrowsException(int capacity)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Cache.Instance.SetMaxCacheItems(capacity));
            Assert.That(Cache.Instance.MaxCacheItems, Is.EqualTo(5));
        }

    }
}
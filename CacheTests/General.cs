using CacheTask;
using CacheTests.Helpers;

namespace CacheTests.Tests
{
    public class CacheTests
    {
        [SetUp]
        public void Setup()
        {
            Cache.Instance.SetMaxCacheItems(5);
            Cache.Instance.Clear(); // remove all items from cache before each test
        }

        [Test]
        public void AddItem_NullOrEmptyKey_ExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(() => Cache.Instance.AddOrUpdate("", "value1"));
            Assert.That(Cache.Instance.Length, Is.EqualTo(0));

            Assert.Throws<ArgumentNullException>(() => Cache.Instance.AddOrUpdate(null, "value1"));
            Assert.That(Cache.Instance.Length, Is.EqualTo(0));

            Assert.Throws<ArgumentNullException>(() => Cache.Instance.AddOrUpdate(" ", "value1"));
            Assert.That(Cache.Instance.Length, Is.EqualTo(0));
        }

        [Test]
        public void AddItem_NullValue_StoredCorrectly()
        {
            Cache.Instance.AddOrUpdate("key1", null);
            Assert.That(Cache.Instance.Get("key1"), Is.EqualTo(null));
        }


        [Test]
        public void AddItem_SingleStringItem_StoredCorrectly()
        {
            Cache.Instance.AddOrUpdate("key1", "value1");
            Assert.That(Cache.Instance.Get("key1"), Is.EqualTo("value1"));
        }

        [Test]
        public void AddItem_MultipleStringItems_StoresAllCorrectly()
        { 
            CacheTestUtilities.GenerateCacheItems(5);

            Assert.That(Cache.Instance.Get("key1"), Is.EqualTo("value1"));
            Assert.That(Cache.Instance.Get("key2"), Is.EqualTo("value2"));
            Assert.That(Cache.Instance.Get("key3"), Is.EqualTo("value3"));
            Assert.That(Cache.Instance.Get("key4"), Is.EqualTo("value4"));
            Assert.That(Cache.Instance.Get("key5"), Is.EqualTo("value5"));
        }


        [TestCase(1)]
        [TestCase(100)]
        [TestCase(2000)]
        public void AddItem_MultipleIntItems_StoresAllCorrectly(int itemsToAdd)
        {
            Cache.Instance.SetMaxCacheItems(itemsToAdd); 

            CacheTestUtilities.GenerateIntCacheItems(itemsToAdd); 

            for (int i = 1; i <= itemsToAdd; i++)
                Assert.That(Cache.Instance.Get($"key{i}"), Is.EqualTo(i));
        }


        [Test]
        public void AddItems_OverCapacityLimit_LastItemStoredCorrectly()
        {
            Cache.Instance.SetMaxCacheItems(1);
            Assert.That(Cache.Instance.MaxCacheItems, Is.EqualTo(1));

            Cache.Instance.AddOrUpdate("key1", "value1");
            Cache.Instance.AddOrUpdate("key2", "value2");
            Cache.Instance.AddOrUpdate("key3", "value3");

            Assert.That(Cache.Instance.Length, Is.EqualTo(1));
            Assert.That(Cache.Instance.Get("key1"), Is.Null);
            Assert.That(Cache.Instance.ContainsKey("key1"), Is.False);
            Assert.That(Cache.Instance.Get("key2"), Is.Null);
            Assert.That(Cache.Instance.ContainsKey("key2"), Is.False);
            Assert.That(Cache.Instance.Get("key3"), Is.EqualTo("value3"));
        }


        [Test]
        public void AddItems_OverCapacityLimit_LeastAccessedAreRemoved()
        {
            CacheTestUtilities.GenerateCacheItems(5);

            Assert.That(Cache.Instance.Length, Is.EqualTo(5));

            // access keys so that order is 2, 4, 5, 1, 3 where 3 is least accessed
            Cache.Instance.Get("key1");
            Cache.Instance.Get("key5");
            Cache.Instance.Get("key4");
            Cache.Instance.Get("key2");

            Cache.Instance.AddOrUpdate("key6", "value6");
            Assert.That(Cache.Instance.Length, Is.EqualTo(5));

            // check key3 was removed
            Assert.That(Cache.Instance.Get("key3"), Is.Null);
            Assert.That(Cache.Instance.ContainsKey("key3"), Is.False);
        }


        [Test]
        public void AddItems_SameKeyDifferentValues_StoredValueIsUpdated()
        {
            Cache.Instance.AddOrUpdate("key1", "value1");
            Assert.That(Cache.Instance.Get("key1"), Is.EqualTo("value1"));

            Cache.Instance.AddOrUpdate("key1", "value1-update-1");
            Assert.That(Cache.Instance.Length, Is.EqualTo(1));
            Assert.That(Cache.Instance.Get("key1"), Is.EqualTo("value1-update-1"));

            Cache.Instance.AddOrUpdate("key1", "value1-update-2");
            Assert.That(Cache.Instance.Length, Is.EqualTo(1));
            Assert.That(Cache.Instance.Get("key1"), Is.EqualTo("value1-update-2"));
        }



        [Test]
        public void GetItem_NullOrEmptyKey_ExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(() => Cache.Instance.Get("")); 
            Assert.Throws<ArgumentNullException>(() => Cache.Instance.Get(null));
            Assert.Throws<ArgumentNullException>(() => Cache.Instance.Get(" "));
        }

        [Test]
        public void GetItem_ExistingItem_GetsCorrectItem()
        {
            CacheTestUtilities.GenerateCacheItems(5);

            Assert.That(Cache.Instance.Get("key3"), Is.EqualTo("value3"));
        }

        [Test]
        public void GetItem_NonExistingItem_ReturnsNull()
        {
            CacheTestUtilities.GenerateCacheItems(5);

            Assert.That(Cache.Instance.Get("key6"), Is.Null);
        }

        [Test]
        public void RemoveItem_NullOrEmptyKey_ExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(() => Cache.Instance.Remove(""));
            Assert.Throws<ArgumentNullException>(() => Cache.Instance.Remove(null));
            Assert.Throws<ArgumentNullException>(() => Cache.Instance.Remove(" "));
        }

        [Test]
        public void RemoveItem_ExistingItem_RemovesCorrectly()
        {  
            CacheTestUtilities.GenerateCacheItems(5);

            Assert.That(Cache.Instance.Get("key3"), Is.EqualTo("value3"));

            Cache.Instance.Remove("key3");
            Assert.That(Cache.Instance.Length, Is.EqualTo(4));
            Assert.That(Cache.Instance.Get("key3"), Is.Null);
        }

        [Test]
        public void RemoveItem_NonExistingItem_DoesNothing()
        { 
            CacheTestUtilities.GenerateCacheItems(5);

            Assert.That(Cache.Instance.Get("key6"), Is.Null);

            Cache.Instance.Remove("key6");
            Assert.That(Cache.Instance.Length, Is.EqualTo(5));
            Assert.That(Cache.Instance.Get("key6"), Is.Null);
        }

        [Test]
        public void ContainsKey_ExistingItem_ReturnsTrue()
        {
            Cache.Instance.AddOrUpdate("key1", "value1");
            Assert.That(Cache.Instance.ContainsKey("key1"), Is.EqualTo(true));
        }

        [Test]
        public void ContainsKey_NonExistingItem_ReturnsFalse()
        {
            // no items added so key0 does not exist
            Assert.That(Cache.Instance.ContainsKey("key0"), Is.EqualTo(false));

            // add items then see if key0 exists
            CacheTestUtilities.GenerateCacheItems(5);

            Assert.That(Cache.Instance.ContainsKey("key0"), Is.EqualTo(false));
        }


        [Test]
        public void UpdateItem_ExistingItem_UpdatesCorrectly()
        {
            // add items then see if key0 exists
            CacheTestUtilities.GenerateCacheItems(5);

            Assert.That(Cache.Instance.Length, Is.EqualTo(5));
            Assert.That(Cache.Instance.Get("key3"), Is.EqualTo("value3"));

            Cache.Instance.AddOrUpdate("key3", "value3-updated");
            Assert.That(Cache.Instance.Get("key3"), Is.EqualTo("value3-updated"));
            Assert.That(Cache.Instance.Length, Is.EqualTo(5));
        }


        [TestCase(1)]
        [TestCase(100)]
        [TestCase(3456)]
        public void GetCacheSize_ReturnsCorrectSize(int itemsToAdd)
        {
            Cache.Instance.SetMaxCacheItems(itemsToAdd);

            // add items then see if key0 exists
            CacheTestUtilities.GenerateCacheItems(itemsToAdd);

            Assert.That(Cache.Instance.Length, Is.EqualTo(itemsToAdd));
        }


        [TestCase(2, 1)]
        [TestCase(100, 50)]
        [TestCase(3456, 99)]
        public void GetCacheSize_NumberOfItemsGreaterThanCapacity_ReturnsCorrectSize(int itemsToAdd, int capacity)
        {
            Cache.Instance.SetMaxCacheItems(capacity);

            // add items then see if key0 exists
            CacheTestUtilities.GenerateCacheItems(itemsToAdd);

            Assert.That(Cache.Instance.Length, Is.EqualTo(capacity));
        }


        [TestCase(2)]
        [TestCase(100)]
        [TestCase(3456)]
        public void ClearCache_EmptiesAllItems(int itemsToAdd)
        {
            Cache.Instance.SetMaxCacheItems(itemsToAdd);
            CacheTestUtilities.GenerateCacheItems(itemsToAdd);
            Cache.Instance.Clear();
            Assert.That(Cache.Instance.Length, Is.EqualTo(0));
        }



    }
}
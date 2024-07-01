
namespace CacheTask
{
    public class Cache
    {
        /// <summary>
        /// Stores cache items and their values with O(1) time complexity
        /// </summary>
        private Dictionary<string, object> _cacheItems;

        /// <summary>
        /// Stores the position of the cache items in the order they were accessed so we can evict the least accessed item
        /// First item is the most recently accessed
        /// </summary>
        private LinkedList<string> _cacheOrder;

        private int _maxCacheItems;
        private object _lockObject = new object(); // TODO: use ReaderWriterLockSlim
        private static readonly Lazy<Cache> instance = new Lazy<Cache>(() => new Cache());


        /// <summary>
        /// Public property to provide access to the singleton instance
        /// </summary>
        public static Cache Instance
        {
            get
            {
                return instance.Value;
            }
        }

        private Cache()
        {
            // default max cache items
            _maxCacheItems = 100;
            _cacheItems = new Dictionary<string, object>();
            _cacheOrder = new LinkedList<string>();
        }


        /// <summary>
        /// Set the maximum number of items that can be stored in the cache, default is 100
        /// </summary>
        /// <param name="maxCacheItems"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetMaxCacheItems(int maxCacheItems)
        {
            if (maxCacheItems <= 0)
                throw new ArgumentOutOfRangeException("maxCacheItems", "maxCacheItems must be greater than 0");

            _maxCacheItems = maxCacheItems;

            // remove items from cache if limit is less than current cache items
            // we could throw an exception here if we want to keep the cache items
            if (_cacheItems.Count > _maxCacheItems)
            {
                var itemsToRemove = _cacheItems.Count - _maxCacheItems;
                for (int i = 0; i < itemsToRemove; i++)
                {
                    var lastKey = _cacheOrder.Last.Value;
                    if (lastKey != null)
                    {
                        _cacheItems.Remove(lastKey);
                        _cacheOrder.Remove(lastKey);
                        OnItemEvicted?.Invoke(this, lastKey);
                    }
                }
            }
        }

        public int MaxCacheItems { get => _maxCacheItems; }

        public int Length { get => _cacheItems.Count; }

        /// <summary>
        /// Returns the key of the evicted item
        /// </summary>
        public event EventHandler<string> OnItemEvicted;


        /// <summary>
        /// Add a new item to the cache or update an existing one if it already exists
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddOrUpdate(string key, object value)
        {
            if (String.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            lock (_lockObject)
            {
                if (_cacheItems.ContainsKey(key))
                {
                    _cacheItems[key] = value;

                    // move to first in the order
                    MoveToFirst(key);
                }
                else
                {
                    if (_cacheItems.Count >= _maxCacheItems)
                    {
                        // remove the last item i.e least accessed
                        var lastKey = _cacheOrder.Last.Value;
                        _cacheItems.Remove(lastKey);

                        // remove from cache order too
                        _cacheOrder.Remove(lastKey);

                        // trigger the event
                        OnItemEvicted?.Invoke(this, lastKey);
                    }

                    _cacheItems.TryAdd(key, value);
                    _cacheOrder.AddFirst(key);
                }
            }
        }

        /// <summary>
        /// Get an item from the cache if it exists
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public object Get(string key)
        {
            if (String.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            lock (_lockObject)
            {
                if (!_cacheItems.ContainsKey(key))
                    return null;

                // move to first in the order
                MoveToFirst(key);

                return _cacheItems[key];
            }
        }


        /// <summary>
        /// Remove an item from the cache if it exists
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Remove(string key)
        {
            if (String.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));


            lock (_lockObject)
            {
                if (_cacheItems.ContainsKey(key))
                {
                    _cacheItems.Remove(key);
                    _cacheOrder.Remove(key);
                }
            }
        }

        /// <summary>
        /// Returns true if the key exists in the cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool ContainsKey(string key)
        {
            if (String.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            lock (_lockObject)
            {
                return _cacheItems.ContainsKey(key);
            }
        }


        /// <summary>
        /// Romove all items in the cache
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                _cacheItems.Clear();
                _cacheOrder.Clear();
            }
        }

        /// <summary>
        /// Move the key to the first in the order, first is the most recently accessed
        /// </summary>
        /// <param name="key"></param>
        private void MoveToFirst(string key)
        {
            _cacheOrder.Remove(key);
            _cacheOrder.AddFirst(key);
        }

    }
}

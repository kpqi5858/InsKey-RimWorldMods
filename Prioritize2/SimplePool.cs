using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prioritize2
{
    public class SimplePool<T>
    {
        private readonly ConcurrentBag<T> objectsInt;
        private readonly Func<T> objectCreator;

        public SimplePool(Func<T> objectGenerator)
        {
            objectCreator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
            objectsInt = new ConcurrentBag<T>();
        }

        public T Get() => objectsInt.TryTake(out T item) ? item : objectCreator();

        public void Release(T item) => objectsInt.Add(item);
    }
}

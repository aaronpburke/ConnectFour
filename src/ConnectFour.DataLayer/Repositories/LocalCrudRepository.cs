using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ConnectFour.DataLayer.Repositories
{
    /// <summary>
    /// An in-memory local collection of objects.
    /// NOTE: This is not a thread-safe implementation!
    /// TODO: This should be replaced with an actual database.
    /// </summary>
    public abstract class LocalCrudRepository<T> : KeyedCollection<string, T> where T : class
    {
        /// <summary>
        /// Adds a new item to the collection.
        /// </summary>
        /// <param name="item">New item to add.</param>
        /// <returns>true if the item was added; otherwise false (e.g., an item with that key already exists)</returns>
        public new bool Add(T item)
        {
            if (this.Contains(GetKeyForItem(item)))
            {
                return false;
            }

            base.Add(item);
            return true;
        }

        /// <summary>
        /// Removes an item from the collection.
        /// </summary>
        /// <param name="id">ID of the item to remove.</param>
        /// <returns>true if the item was removed; otherwise false (e.g., the item did not exist)</returns>
        public bool Delete(string id)
        {
            if (this.Contains(id))
            {
                return false;
            }

            base.Remove(id);
            return true;
        }

        /// <summary>
        /// Gets a item from the collection by name.
        /// </summary>
        /// <param name="itemName">Name of the item to return.</param>
        /// <returns>item if it exists in the collection; otherwise null.</returns>
        public T Get(string itemName)
        {
            return this.TryGetValue(itemName, out var item) ? item : null;
        }

        /// <summary>
        /// Gets all items currently in the collection.
        /// </summary>
        /// <returns>All items in the collection.</returns>
        public IQueryable<T> GetAll()
        {
            return this.Items.AsQueryable<T>();
        }

        /// <summary>
        /// Updates the collection and replaces an item element.
        /// </summary>
        /// <param name="item">Replacement item.</param>
        /// <returns>true if the item previously existed and was updated; otherwise false (i.e., the item did not previously exist).</returns>
        public bool Update(T item)
        {
            try
            {
                int index = this.IndexOf(this[GetKeyForItem(item)]);
                if (index < 0)
                {
                    return false;
                }

                this.SetItem(index, item);
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }
    }
}

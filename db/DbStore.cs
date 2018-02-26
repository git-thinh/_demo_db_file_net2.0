using System;
using System.Collections.Generic;
using System.Text; 
using System.Collections;
using System.Threading;
using System.Linq; 

namespace Core
{
    public interface IDB
    {
        int Item_AddOrUpdate(object item);
        //bool Item_Get<T>(Func<T, bool> condition);
        //bool Item_Remove<T>(T obj, Func<T, bool> condition);
    }

    public class DbStore : IDB, IStoreType
    {
        private readonly ILog log;

        private readonly object _lockType;
        private readonly Dictionary<string, Type> storeType;
        private readonly List<string> listType;

        private readonly object _lockRW;
        private readonly Dictionary<int, ReaderWriterLockSlim> storeLock;
        private readonly Dictionary<int, IList> storeData;

        public DbStore(ILog _log)
        {
            log = _log;

            listType = new List<string>();

            _lockType = new object();
            storeType = new Dictionary<string, Type>();

            _lockRW = new object();
            storeLock = new Dictionary<int, ReaderWriterLockSlim>();
            storeData = new Dictionary<int, IList>();
        }

        #region [ === STORE TYPE === ]

        public int Type_AddOrUpdate(Type type)
        {
            if (type == null) return;
            string key = type.FullName;

            int index = -1;
            lock (_lockType)
            {
                if (storeType.ContainsKey(key))
                    storeType[type.FullName] = type;
                else
                {
                    storeType.Add(key, type);
                    listType.Add(key);
                    index = listType.Count - 1;
                }
            }
            if (index >= 0)
            {
                lock (_lockRW)
                {
                    storeLock.Add(index, new ReaderWriterLockSlim());
                    IList list = CreateInstanceList(type);
                    storeData.Add(index, list);
                }
            }
        }

        public Type Type_Get(string type_Name)
        {
            if (type_Name == null) return null;
            Type type;
            lock (_lockType)
                storeType.TryGetValue(type_Name, out type);
            return type;
        }

        #endregion

        private int get_IndexStore(string key) 
        {
            int index = -1;
            if (string.IsNullOrEmpty(key)) return index;
            lock (_lockType)
                index = listType.IndexOf(key);
            return index;
        }

        public bool Item_Remove(object item)
        {
            int index = -1;
            if (item == null) return false;

            Type type = item.GetType();
            string key = type.FullName;

            index = get_IndexStore(key);
            if (index > 0)
            {
                IList list = null;
                ReaderWriterLockSlim rw = null;

                lock (_lockRW)
                    storeLock.TryGetValue(index, out rw);

                if (rw != null)
                {
                    using (rw.WriteLock())
                    {
                        if (storeData.TryGetValue(index, out list))
                        {
                            int index_it = list.IndexOf(item);
                            if (index_it != -1)
                            {
                                list[index_it] = null;
                                storeData[index] = list;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public int Item_AddOrUpdate(object item)
        {
            int id = -1;
            if (item == null) return id;

            Type type = item.GetType();
            string key = type.FullName;

            int index = get_IndexStore(key);
            if (index > 0)
            {
                IList list = null;
                ReaderWriterLockSlim rw = null;

                lock (_lockRW)
                    storeLock.TryGetValue(index, out rw);

                if (rw != null) 
                {
                    using (rw.WriteLock()) 
                    {
                        if (storeData.TryGetValue(index, out list)) 
                        {
                            list.Add(item);
                            storeData[index] = list;
                            id = list.Count - 1;
                        }
                    }
                }
            }
            return index;
        }

        private IList CreateInstanceList(Type type)
        {
            var listType = typeof(List<>);
            var constructedListType = listType.MakeGenericType(type);
            var instance = Activator.CreateInstance(constructedListType);
            return (IList)instance;
        }
         
    }
}

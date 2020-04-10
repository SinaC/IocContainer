using System;
using System.Collections.Generic;

namespace EasyIoc
{
    internal sealed class TypeStorageCollection<TEntity>
        where TEntity: class
    {
        private sealed class TypeStorageCollectionEntry
        {
            public TEntity Anonymous { get; set; }
            public Dictionary<string, TEntity> Named { get; } = new Dictionary<string, TEntity>();
        }

        private readonly Dictionary<Type, TypeStorageCollectionEntry> _dictionary = new Dictionary<Type, TypeStorageCollectionEntry>();

        public bool UnsafeContainsKey(Type interfaceType, string name)
        {
            if (!_dictionary.TryGetValue(interfaceType, out var entity))
                return false;
            if (name == null)
                return entity.Anonymous != default(TEntity);
            return entity.Named.ContainsKey(name);
        }

        public bool UnsafeTryGet(Type interfaceType, string name, out TEntity value)
        {
            if (!_dictionary.TryGetValue(interfaceType, out var entity))
            {
                value = default(TEntity);
                return false;
            }

            if (name == null)
            {
                value = entity.Anonymous;
                return value != default(TEntity);
            }

            return entity.Named.TryGetValue(name, out value);
        }

        public void UnsafeAdd(Type interfaceType, string name, TEntity value)
        {
            if (!_dictionary.TryGetValue(interfaceType, out var entity))
            {
                entity = new TypeStorageCollectionEntry();
                _dictionary.Add(interfaceType, entity);
            }

            if (name == null)
                entity.Anonymous = value;
            else
                entity.Named[name] = value;
        }

        public bool UnsafeRemove(Type interfaceType, string name)
        {
            if (!_dictionary.TryGetValue(interfaceType, out var entity))
                return false;
            if (name == null)
            {
                entity.Anonymous = default(TEntity);
                return true;
            }

            return entity.Named.Remove(name);
        }

        public bool UnsafeRemove(Type type)
        {
            return _dictionary.Remove(type);
        }

        public void UnsafeClear()
        {
            _dictionary.Clear();
        }
    }
}

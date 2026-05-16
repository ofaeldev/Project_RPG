using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGProject.Systems
{
    public sealed class ComponentPool<T> where T : Component
    {
        private readonly Stack<T> available = new();
        private readonly HashSet<T> pooled = new();
        private readonly Func<T> create;
        private readonly Transform parent;

        public ComponentPool(Func<T> create, Transform parent)
        {
            this.create = create;
            this.parent = parent;
        }

        public T Get()
        {
            T instance = available.Count > 0 ? available.Pop() : Create();
            if (instance != null)
            {
                instance.gameObject.SetActive(true);
            }

            return instance;
        }

        public void Release(T instance)
        {
            if (instance == null || !pooled.Contains(instance))
            {
                return;
            }

            instance.gameObject.SetActive(false);
            if (parent != null)
            {
                instance.transform.SetParent(parent, true);
            }

            available.Push(instance);
        }

        public void Clear()
        {
            foreach (T instance in pooled)
            {
                if (instance != null)
                {
                    UnityEngine.Object.Destroy(instance.gameObject);
                }
            }

            available.Clear();
            pooled.Clear();
        }

        private T Create()
        {
            T instance = create != null ? create() : null;
            if (instance == null)
            {
                return null;
            }

            pooled.Add(instance);
            if (parent != null)
            {
                instance.transform.SetParent(parent, true);
            }

            return instance;
        }
    }
}

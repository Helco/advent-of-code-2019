using System;
using System.Collections.Generic;
using System.Linq;

// Implementation from here https://stackoverflow.com/a/4994931
// slightly changed to be typesafe
// add ChangePrio method

namespace day15
{
    public class PrioQueue<T>
    {
        int total_size;
        SortedDictionary<int, Queue<T>> storage;

        public PrioQueue()
        {
            this.storage = new SortedDictionary<int, Queue<T>>();
            this.total_size = 0;
        }

        public bool IsEmpty()
        {
            return (total_size == 0);
        }

        public T Dequeue()
        {
            if (IsEmpty())
            {
                throw new Exception("Please check that priorityQueue is not empty before dequeing");
            }
            else
                foreach (Queue<T> q in storage.Values)
                {
                    // we use a sorted dictionary
                    if (q.Count > 0)
                    {
                        total_size--;
                        return q.Dequeue();
                    }
                }

            throw new InvalidProgramException("Should not reach here");
        }

        // same as above, except for peek.

        public T Peek()
        {
            if (IsEmpty())
                throw new Exception("Please check that priorityQueue is not empty before peeking");
            else
                foreach (Queue<T> q in storage.Values)
                {
                    if (q.Count > 0)
                        return q.Peek();
                }

            throw new InvalidProgramException("Should not reach here");
        }

        public void Enqueue(T item, int prio)
        {
            if (!storage.ContainsKey(prio))
            {
                storage.Add(prio, new Queue<T>());
            }
            storage[prio].Enqueue(item);
            total_size++;

        }

        public void ChangePrio(T item, int prevPrio, int newPrio)
        {
            if (!storage.TryGetValue(prevPrio, out Queue<T> queue))
                return;
            storage[prevPrio] = new Queue<T>(queue.Except(new T[] { item }));
            total_size--;
            Enqueue(item, newPrio);
        }
    }
}
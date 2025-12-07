
// (c) 2025 Kazuki Kohzuki

using System.Collections;

namespace TAFitting.Collections;

/// <summary>
/// Represents a first-in, first-out (FIFO) collection with a fixed maximum size.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class FixedSizeQueue<T> : IEnumerable<T>
{
    private readonly Queue<T> _queue;

    /// <summary>
    /// Gets the maximum number of elements the <see cref="FixedSizeQueue{T}"/> can hold.
    /// </summary>
    internal int Capacity { get; }

    /// <summary>
    /// Gets the number of elements contained in the <see cref="FixedSizeQueue{T}"/>.
    /// </summary>
    internal int Count => this._queue.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="FixedSizeQueue{T}"/> class with the specified capacity.
    /// </summary>
    /// <param name="capacity">The maximum number of elements the queue can hold.</param>
    internal FixedSizeQueue(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        this.Capacity = capacity;
        this._queue = new(capacity);
    } // ctor (int)

    /// <summary>
    /// Enqueues an item to the end of the <see cref="FixedSizeQueue{T}"/>.
    /// </summary>
    /// <param name="item">The item to enqueue.</param>
    /// <remarks>
    /// If the queue is at full capacity, the oldest item will be removed to make space for the new item.
    /// </remarks>
    internal void Enqueue(T item)
    {
        // Remove the oldest item first, then add the new item.
        // Reverse order results in capacity increasing, which causes unnecessary memory allocation.
        if (this._queue.Count == this.Capacity)
            _ = this._queue.Dequeue();
        this._queue.Enqueue(item);
    } // internal void Enqueue (T)

    /// <summary>
    /// Dequeues and returns the item at the front of the <see cref="FixedSizeQueue{T}"/>.
    /// </summary>
    /// <returns>The item that was removed from the front of the queue.</returns>
    internal T Dequeue()
        => this._queue.Dequeue();

    /// <summary>
    /// Returns the item at the front of the <see cref="FixedSizeQueue{T}"/> without removing it.
    /// </summary>
    /// <returns>The item at the front of the queue.</returns>
    internal T Peek()
        => this._queue.Peek();

    /// <summary>
    /// Removes all items from the <see cref="FixedSizeQueue{T}"/>.
    /// </summary>
    internal void Clear()
        => this._queue.Clear();

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
        => this._queue.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => this._queue.GetEnumerator();
} // internal class FixedSizeQueue<T> : IEnumerable<T>

﻿//
// EventHandlingCollectionBase.cs
//
// Author:
//       Craig Fowler <craig@craigfowler.me.uk>
//
// Copyright (c) 2016 Craig Fowler
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace CSF.Collections.EventRaising
{
  /// <summary>
  /// Base type for event-handling collections.
  /// </summary>
  [Serializable]
  public abstract class EventRaisingCollectionBase<TItem> : IEventRaisingCollection<TItem>
    where TItem : class
  {
    #region fields

    private readonly ICollection<TItem> _sourceCollection;

    #endregion

    #region properties

    /// <summary>
    /// Gets the wrapped source collection instance.
    /// </summary>
    /// <value>The source collection.</value>
    protected ICollection<TItem> SourceCollection
    {
      get {
        return _sourceCollection;
      }
    }

    #endregion

    #region ICollection implementation

    /// <summary>
    /// Gets the count of elements in this collection.
    /// </summary>
    /// <value>
    /// The count.
    /// </value>
    public virtual int Count
    {
      get {
        return SourceCollection.Count;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is read only.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
    /// </value>
    public virtual bool IsReadOnly
    {
      get {
        return SourceCollection.IsReadOnly;
      }
    }

    /// <summary>
    /// Gets the enumerator for the current instance.
    /// </summary>
    /// <returns>
    /// The enumerator.
    /// </returns>
    public virtual IEnumerator<TItem> GetEnumerator()
    {
      return SourceCollection.GetEnumerator();
    }

    /// <summary>
    /// Adds an item to the current instance.
    /// </summary>
    /// <param name='item'>
    /// The item to add.
    /// </param>
    public virtual void Add(TItem item)
    {
      if(HandleBeforeAdd(item))
      {
        SourceCollection.Add(item);
        HandleAfterAdd(item);
      }
    }

    /// <summary>
    /// Clears all items from the current instance.
    /// </summary>
    public virtual void Clear()
    {
      while(SourceCollection.Any())
      {
        Remove(SourceCollection.First());
      }
    }

    /// <summary>
    /// Determines whether the current collection contains a specific value.
    /// </summary>
    /// <param name='item'>
    /// The item to search for.
    /// </param>
    public virtual bool Contains(TItem item)
    {
      return SourceCollection.Contains(item);
    }

    /// <summary>
    /// Copies the contents of the current instance to an array.
    /// </summary>
    /// <param name='array'>
    /// The array to copy to.
    /// </param>
    /// <param name='arrayIndex'>
    /// Array index.
    /// </param>
    public virtual void CopyTo(TItem[] array, int arrayIndex)
    {
      SourceCollection.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Removes the first occurrence of an item from the current collection.
    /// </summary>
    /// <param name='item'>
    /// The item to remove from the current collection.
    /// </param>
    public virtual bool Remove(TItem item)
    {
      if(HandleBeforeRemove(item))
      {
        var output = SourceCollection.Remove(item);

        if(output)
        {
          HandleAfterRemove(item);
        }

        return output;
      }

      return false;
    }

    #endregion

    #region explicit interface implementations

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }

    void ICollection.CopyTo(Array array, int index)
    {
      TItem[] copy = new TItem[SourceCollection.Count];
      CopyTo(copy, 0);
      Array.Copy(copy, 0, array, index, SourceCollection.Count);
    }

    object ICollection.SyncRoot
    {
      get {
        return ((ICollection) SourceCollection).SyncRoot;
      }
    }

    bool ICollection.IsSynchronized
    {
      get {
        return ((ICollection) SourceCollection).IsSynchronized;
      }
    }

    #endregion

    #region events

    /// <summary>
    /// Occurs before an item is added to the collection.
    /// </summary>
    public event EventHandler<BeforeModifyEventArgs<TItem>> BeforeAdd;

    /// <summary>
    /// Occurs after an item is added to the collection.
    /// </summary>
    public event EventHandler<AfterModifyEventArgs<TItem>> AfterAdd;

    /// <summary>
    /// Occurs before an item is removed the collection.
    /// </summary>
    public event EventHandler<BeforeModifyEventArgs<TItem>> BeforeRemove;

    /// <summary>
    /// Occurs after an item is removed from the collection.
    /// </summary>
    public event EventHandler<AfterModifyEventArgs<TItem>> AfterRemove;

    #endregion

    #region protected methods

    /// <summary>
    /// Raises the <see cref="BeforeAdd"/> event.
    /// </summary>
    /// <returns><c>true</c>, if it is OK for the add-item to continue, <c>false</c> if it is has been cancelled.</returns>
    /// <param name="item">The item to be added.</param>
    protected bool HandleBeforeAdd(TItem item)
    {
      var args = CreateBeforeActionEventArgs(item);
      BeforeAdd?.Invoke(this, args);

      var cancelable = args as ICancelable;
      return (cancelable != null)? !cancelable.IsCancelled : true;
    }

    /// <summary>
    /// Raises the <see cref="AfterAdd"/> event.
    /// </summary>
    /// <param name="item">The item to be added.</param>
    protected void HandleAfterAdd(TItem item)
    {
      var args = CreateAfterActionEventArgs(item);
      AfterAdd?.Invoke(this, args);
    }

    /// <summary>
    /// Raises the <see cref="BeforeRemove"/> event.
    /// </summary>
    /// <returns><c>true</c>, if it is OK for the remove-item to continue, <c>false</c> if it is has been cancelled.</returns>
    /// <param name="item">The item to be removed.</param>
    protected bool HandleBeforeRemove(TItem item)
    {
      var args = CreateBeforeActionEventArgs(item);
      BeforeRemove?.Invoke(this, args);

      var cancelable = args as ICancelable;
      return (cancelable != null)? !cancelable.IsCancelled : true;
    }

    /// <summary>
    /// Raises the <see cref="AfterRemove"/> event.
    /// </summary>
    /// <param name="item">The associated item.</param>
    protected void HandleAfterRemove(TItem item)
    {
      var args = CreateAfterActionEventArgs(item);
      AfterRemove?.Invoke(this, args);
    }

    /// <summary>
    /// Creates a set of appropriately-populated before-action event arguments.
    /// </summary>
    /// <returns>The before-action event arguments.</returns>
    /// <param name="item">Item.</param>
    protected abstract BeforeModifyEventArgs<TItem> CreateBeforeActionEventArgs(TItem item);

    /// <summary>
    /// Creates a set of appropriately-populated after-action event arguments.
    /// </summary>
    /// <returns>The after-action event arguments.</returns>
    /// <param name="item">The associated item.</param>
    protected abstract AfterModifyEventArgs<TItem> CreateAfterActionEventArgs(TItem item);

    #endregion

    #region constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="T:EventHandlingCollectionBase{TItem}"/> class.
    /// </summary>
    /// <param name='source'>The source collection that this instance wraps.</param>
    public EventRaisingCollectionBase(ICollection<TItem> source)
    {
      if(source == null)
      {
        throw new ArgumentNullException(nameof(source));
      }

      _sourceCollection = source;
    }

    #endregion
  }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

public class LruCache {

  private Hashtable     data           = new Hashtable();
  private List<object>  recentKeys     = new List<object>();
  private Hashtable     recentKeyIndex = new Hashtable();
  private int           maxSize;

  public
  LruCache(int maxSize) {
    this.maxSize   = maxSize;
  }

  public void
  Set(object key, object val) {
    Touch(key);
    data[key] = val;
    DeleteOldestIfNeeded();
  }

  public object
  Get(object key) {
    object val = data[key];
    if (val != null) {
      Touch(key);
    }
    return val;
  }

  private void
  Touch(object key) {
    if (recentKeyIndex.ContainsKey(key)) {
      recentKeys.RemoveAt((int)recentKeyIndex[key]);
    }
    recentKeys.Add(key);
    recentKeyIndex[key] = recentKeys.Count - 1;
  }

  private void
  DeleteOldestIfNeeded() {
    if (recentKeys.Count > maxSize) {
      object key = recentKeys[0];
      recentKeyIndex.Remove(key);
      data.Remove(key);
      recentKeys.RemoveAt(0);
    }
  }
}

using NUnit.Framework;

[TestFixture] public class LruCacheTest {
  private int       lruSize = 3;
  private LruCache  lru;

  [SetUp] public void SetUp() {
    lru   = new LruCache(lruSize);
  }

  [Test] public void
  ShouldReturnItemsEnteredUpToLruSize() {
    lru.Set("a", "one");
    lru.Set("b", 2);
    lru.Set("c", 3);
    Assert.AreEqual("one", lru.Get("a"));
    Assert.AreEqual(2,     lru.Get("b"));
    Assert.AreEqual(3,     lru.Get("c"));
  }

  [Test] public void
  ShouldReturnNullForMissingItems() {
    Assert.IsNull(lru.Get("bogus"));
  }

  [Test] public void
  ShouldForgetItemsBeyondLruSize() {
    lru.Set("a", 1); // [a]
    lru.Set("b", 2); // [a, b]
    lru.Set("c", 3); // [a, b, c]
    lru.Set("d", 4); // [b, c, d]
    Assert.IsNull(lru.Get("a"));
  }

  [Test] public void
  ShouldNotExpireRecentlyRetrievedItems() {
    lru.Set("a", 1); // [a]
    lru.Set("b", 2); // [a, b]
    lru.Set("c", 3); // [a, b, c]
    lru.Get("a");    // [b, c, a]
    lru.Set("d", 4); // [c, a, d]
    Assert.AreEqual(1, lru.Get("a"));
    Assert.IsNull(lru.Get("b"));
  }

  [Test] public void
  ShouldNotExhibitOutOfRangeBugWhenReAddingTheSameKeyAfterExpiry() {
    lru.Set("a", 1);
    lru.Set("b", 1);
    lru.Set("c", 1);
    lru.Set("d", 1);
    lru.Set("a", 1);
    lru.Set("a", 1);
  }

  [Test] public void
  ShouldCalculateCorrectKeyIndexOverMultipleCycles() {
    for (int i = 0; i < 1000; i++) {
      lru.Set("a", 1);
      lru.Set("b", 2);
      lru.Set("c", 3);
      lru.Set("d", 4);
    }
    Assert.AreEqual(4, lru.Get("d"));
    Assert.IsNull(lru.Get("a"));
  }

}

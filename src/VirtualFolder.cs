namespace BBCiPlayer {
  using System;
  using System.Collections;

  public class VirtualFolder {

    private string     title;
    private string     id;
    private string     parentId;
    private string     sourceUrl;
    private bool       dynamic    = false;
    private ArrayList  items      = new ArrayList();
    private Hashtable  lookup     = new Hashtable();
    private string     filter;
    private string     identifier = null;
    private DateTime   lastLoad   = DateTime.MinValue;

    public
    VirtualFolder(string id, string title)
      : this(id, title, "", false) {}

    public
    VirtualFolder(string id, string title, string sourceUrl)
      : this(id, title, sourceUrl, false) {}

    public
    VirtualFolder(string id, string title, string sourceUrl, bool dynamic) {
      this.id        = id;
      this.title     = title;
      this.sourceUrl = sourceUrl;
      this.dynamic   = dynamic;
      this.parentId  = "-1";
    }

    public string
    Title {
      get { return this.title; }
      set { this.title = value; }
    }

    public string
    Id {
      get { return this.id; }
      set { this.id = value; }
    }

    public string
    ParentId {
      get { return this.parentId; }
      set { this.parentId = value; }
    }

    public ArrayList
    Items {
      get { return this.items; }
    }

    public string
    Filter {
      get { return this.filter; }
      set { this.filter = value; }
    }

    public string
    Identifier {
      get { return this.identifier; }
      set { this.identifier = value; }
    }

    public DateTime
    LastLoad {
      get { return this.lastLoad; }
      set { this.lastLoad = value; }
    }

    public void
    Reset() {
      this.items.Clear();
    }

    public void
    AddMedia(SharedOnlineMediaInfo info) {
      this.items.Add(info);
      info.OwnerId = this.id;
      this.lookup[info.OnlineIdentifier] = info.Id;
    }

    public void
    AddFolder(VirtualFolder folder) {
      this.items.Add(folder);
      folder.ParentId = this.id;
      if (folder.Filter == null || folder.Filter.Length == 0)
        folder.Filter = this.filter;
      if (folder.Identifier != null)
        this.lookup[folder.Identifier] = folder.Id;
    }

    public string
    FindGuid(string onlineIdentifier) {
      return lookup[onlineIdentifier] as string;
    }

    public string
    SourceURL {
      get { return this.sourceUrl; }
      set { this.sourceUrl = value; }
    }

    public bool
    Dynamic {
      get { return this.dynamic; }
      set { this.dynamic = value; }
    }

  }
}

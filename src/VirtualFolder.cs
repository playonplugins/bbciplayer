namespace Beeb {

  using System;
  using System.Collections;
  using MediaMallTechnologies.Plugin;

  public class VirtualFolder {

    public string     Title;
    public string     Id;
    public string     ParentId;
    public string     Filter;
    public string     Identifier = null;
    public DateTime   LastLoad   = DateTime.MinValue;
    public string     SourceUrl;
    public bool       Dynamic    = false;

    private ArrayList  items      = new ArrayList();
    private Hashtable  lookup     = new Hashtable();

    ////

    public
    VirtualFolder(string id, string title)
      : this(id, title, "", false) {}

    public
    VirtualFolder(string id, string title, string sourceUrl)
      : this(id, title, sourceUrl, false) {}

    public
    VirtualFolder(string id, string title, string sourceUrl, bool dynamic) {
      this.Id        = id;
      this.Title     = title;
      this.SourceUrl = sourceUrl;
      this.Dynamic   = dynamic;
      this.ParentId  = "-1";
    }

    ////

    public ArrayList
    Items {
      get { return this.items; }
    }

    ////

    public void
    Reset() {
      this.items.Clear();
    }

    public void
    AddMedia(SharedMediaFileInfo info) {
      this.items.Add(info);
      info.OwnerId = this.Id;
      this.lookup[info.SourceId] = info.Id;
    }

    public void
    AddFolder(VirtualFolder folder) {
      this.items.Add(folder);
      folder.ParentId = this.Id;
      if (folder.Filter == null || folder.Filter.Length == 0)
        folder.Filter = this.Filter;
      if (folder.Identifier != null)
        this.lookup[folder.Identifier] = folder.Id;
    }

    public string
    FindGuid(string sourceId) {
      return lookup[sourceId] as string;
    }
  }
}

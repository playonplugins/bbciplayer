namespace Beeb {

  using System;
  using System.Collections;
  using MediaMallTechnologies.Plugin;

  public class VirtualFolder {

    public string    Title;
    public string    Id;
    public string    ParentId;
    public string    Filter;
    public string    SourceUrl;
    public string    Identifier = null;
    public DateTime  LastLoad   = DateTime.MinValue;
    public bool      Dynamic    = false;

    public readonly ArrayList Items = new ArrayList();

    private Hashtable lookup = new Hashtable();

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

    public void
    Reset() {
      this.Items.Clear();
    }

    public void
    AddMedia(SharedMediaFileInfo info) {
      this.Items.Add(info);
      info.OwnerId = this.Id;
      this.lookup[info.SourceId] = info.Id;
    }

    public void
    AddFolder(VirtualFolder folder) {
      this.Items.Add(folder);
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

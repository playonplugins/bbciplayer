namespace Beeb {

  using System;
  using System.Collections;

  public class FolderStructure {
    private VirtualFolder  parent;
    private Hashtable      folderLookup;
    private string         path;

    public delegate void FolderDelegate(FolderStructure f);

    ////

    public
    FolderStructure(VirtualFolder parent, Hashtable folderLookup, string path) {
      this.parent       = parent;
      this.folderLookup = folderLookup;
      this.path         = path;
    }

    public static void
    Folder(VirtualFolder parent, Hashtable folderLookup, string path, FolderDelegate subfolderDelegate) {
      FolderStructure f = new FolderStructure(parent, folderLookup, path);
      subfolderDelegate(f);
    }

    ////

    public void
    Category(string name, string path) {
      VirtualFolder folder = new VirtualFolder(CreateGuid(), name);
      AddFolderWithLookup(this.parent, folder);

      string subPath = this.path + "/" + path;
      FolderStructure f = new FolderStructure(folder, this.folderLookup, subPath);
      f.Feed("Most popular " + name,        "popular");
      f.Feed(name + " highlights",          "highlights");
      f.Feed("All " + name + " programmes", "list");
    }

    public void
    Channel(string name, string path) {
      VirtualFolder folder = new VirtualFolder(CreateGuid(), name);
      folder.Thumbnail = "http://www.bbc.co.uk/iplayer/img/station_logos/" + path + ".png";
      AddFolderWithLookup(this.parent, folder);

      string subPath = this.path + "/" + path;
      FolderStructure f = new FolderStructure(folder, this.folderLookup, subPath);
      f.Feed("Most popular on " + name,     "popular");
      f.Feed(name + " highlights",          "highlights");
      f.Feed("All " + name + " programmes", "list");
    }

    public void
    Folder(string name, string path, FolderDelegate subfolderDelegate) {
      string subPath;

      VirtualFolder child = new VirtualFolder(CreateGuid(), name);
      AddFolderWithLookup(this.parent, child);

      if (path == null) {
        subPath = this.path;
      } else {
        subPath = this.path + "/" + path;
      }

      subfolderDelegate(new FolderStructure(child, this.folderLookup, subPath));
    }

    public void
    Folder(string name, FolderDelegate subfolderDelegate) {
      Folder(name, null, subfolderDelegate);
    }

    public void
    Feed(string name, string path) {
      VirtualFolder child = new VirtualFolder(CreateGuid(), name, this.path + "/" + path, true);
      AddFolderWithLookup(this.parent, child);
    }

    ////

    private string
    CreateGuid() {
      return "FolderStructure-" + Guid.NewGuid();
    }

    private void
    AddFolderWithLookup(VirtualFolder parent, VirtualFolder child)
    {
      parent.AddFolder(child);
      this.folderLookup[child.Id] = child;
    }
  }
}


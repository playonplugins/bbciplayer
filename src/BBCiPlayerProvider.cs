namespace Beeb {

  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.IO;
  using System.Net;
  using System.Xml;
  using MediaMallTechnologies.Plugin;

  public class BBCiPlayerProvider : IPlayOnProvider {

    private IPlayOnHost        host;
    private VirtualFolder      rootFolder;
    private Hashtable          titleLookup            = new Hashtable();
    private Hashtable          folderLookup           = new Hashtable();
    private int                dynamicFolderCacheTime = 300; // seconds
    private ProgrammeDatabase  progDB;
    private string             feedRoot               = "http://feeds.bbc.co.uk/iplayer/";

    ////

    public string
    Name {
      get { return "BBC iPlayer"; }
    }

    public string
    ID {
      get { return this.Name.Replace(" ", "").ToLower(); }
    }

    public System.Drawing.Image
    Image {
      get {
        System.Drawing.Image image = null;
        Stream imageStream = System.Reflection.Assembly.GetExecutingAssembly().
                             GetManifestResourceStream("Logo78x78.png");
        if (imageStream != null) {
          image = System.Drawing.Image.FromStream(imageStream);
          imageStream.Close();
        }
        return image;
      }
    }

    ////

    public Payload
    GetSharedMedia(string id, bool includeChildren, int startIndex, int requestCount) {
      this.Log("GetSharedMedia");

      if (id == null || id.Length == 0) {
        return new Payload("-1", "-1", "[Invalid Request]", 0, new List<AbstractSharedMediaInfo>(0));
      }

      List<AbstractSharedMediaInfo> currentList = new List<AbstractSharedMediaInfo>();

      if (id == this.ID) { // root
        foreach (VirtualFolder subFolder in this.rootFolder.Items) {
          subFolder.ParentId = this.ID;
          currentList.Add(new SharedMediaFolderInfo(subFolder.Id, id, subFolder.Title, subFolder.Items.Count,
                                                    subFolder.Thumbnail, default(NameValueCollection)));
        }
        return new Payload(id, "0", this.Name, currentList.Count,
                           GetRange(currentList, startIndex, requestCount));
      }

      if (titleLookup[id] != null) {
        SharedMediaFileInfo fileInfo = (SharedMediaFileInfo)titleLookup[id];
        currentList.Add(fileInfo);
        return new Payload(id, fileInfo.OwnerId, fileInfo.Title, 1, currentList, false);
      }

      if (folderLookup[id] != null) {
        VirtualFolder folder = (VirtualFolder)folderLookup[id];

        if (folder.Dynamic && (DateTime.Now - folder.LastLoad).TotalSeconds > this.dynamicFolderCacheTime) {
          LoadDynamicFolder(folder);
          folder.LastLoad = DateTime.Now;
        }

        foreach (object entry in folder.Items) {
          if (entry is VirtualFolder) {
            VirtualFolder subFolder = entry as VirtualFolder;
            currentList.Add(
              new SharedMediaFolderInfo(subFolder.Id, subFolder.Id, subFolder.Title, subFolder.Items.Count,
                                        subFolder.Thumbnail, default(NameValueCollection)));
          } else if (entry is SharedMediaFileInfo) {
            SharedMediaFileInfo fileInfo = (SharedMediaFileInfo)entry;
            currentList.Add(fileInfo);
          }
        }
        return new Payload(id, folder.ParentId, folder.Title, currentList.Count,
                           GetRange(currentList, startIndex, requestCount));
      }

      return new Payload("-1", "-1", "[Unknown Request]", 0, new List<AbstractSharedMediaInfo>(0));
    }

    public string
    Resolve(SharedMediaFileInfo fileInfo) {
      this.Log("Resolve: " + fileInfo.Path);

      string type = "fp";
      string url = progDB.VpidToStreamingUrl(fileInfo.SourceId);
      this.Log("Resolved to: " + url);

      StringWriter sw = new StringWriter();
      XmlTextWriter writer = new XmlTextWriter(sw);

      writer.Formatting = Formatting.Indented;
      writer.WriteStartElement("media");
      writer.WriteStartElement("url");
      writer.WriteAttributeString("type", type);
      writer.WriteString(url);
      writer.WriteEndElement();
      writer.WriteEndElement();
      writer.Close();
      string xml = sw.ToString();
      Log("Resolved XML: " + xml);

      return xml;
    }

    public void
    SetPlayOnHost(IPlayOnHost h) {
      this.host = h;
      this.progDB = new ProgrammeDatabase();
      AddFolders();
    }

    ////

    private void
    AddFolders() {
      this.rootFolder = new VirtualFolder(this.ID, this.Name);

      AddFolderFromFeed(rootFolder, "Most Popular TV", feedRoot + "popular/tv/list");
      AddFolderFromFeed(rootFolder, "TV Highlights",   feedRoot + "highlights/tv");

      VirtualFolder allChannelsFolder = new VirtualFolder(CreateGuid(), "TV Channels");
      rootFolder.AddFolder(allChannelsFolder);
      this.folderLookup[allChannelsFolder.Id] = allChannelsFolder;

      AddChannelFolder(allChannelsFolder, "BBC One",          "bbc_one");
      AddChannelFolder(allChannelsFolder, "BBC Two",          "bbc_two");
      AddChannelFolder(allChannelsFolder, "BBC Three",        "bbc_three");
      AddChannelFolder(allChannelsFolder, "BBC Four",         "bbc_four");
      AddChannelFolder(allChannelsFolder, "CBBC",             "cbbc");
      AddChannelFolder(allChannelsFolder, "CBeebies",         "cbeebies");
      AddChannelFolder(allChannelsFolder, "BBC News Channel", "bbc_news24");
      AddChannelFolder(allChannelsFolder, "BBC Parliament",   "bbc_parliament");
      AddChannelFolder(allChannelsFolder, "BBC Alba",         "bbc_alba");

      VirtualFolder allCategoriesFolder = new VirtualFolder(CreateGuid(), "TV Categories");
      rootFolder.AddFolder(allCategoriesFolder);
      this.folderLookup[allCategoriesFolder.Id] = allCategoriesFolder;

      AddCategoryFolder(allCategoriesFolder, "Children's",        "categories/childrens/tv");
      AddCategoryFolder(allCategoriesFolder, "Comedy",            "categories/comedy/tv");
      AddCategoryFolder(allCategoriesFolder, "Drama",             "categories/drama/tv");
      AddCategoryFolder(allCategoriesFolder, "Entertainment",     "categories/entertainment/tv");
      AddCategoryFolder(allCategoriesFolder, "Factual",           "categories/factual/tv");
      AddCategoryFolder(allCategoriesFolder, "Films",             "categories/films/tv");
      AddCategoryFolder(allCategoriesFolder, "Learning",          "categories/learning/tv");
      AddCategoryFolder(allCategoriesFolder, "Music",             "categories/Music/tv");
      AddCategoryFolder(allCategoriesFolder, "Drama",             "categories/News/tv");
      AddCategoryFolder(allCategoriesFolder, "Religion & Ethics", "categories/religion_and_ethics/tv");
      AddCategoryFolder(allCategoriesFolder, "Sport",             "categories/sport/tv");
      AddCategoryFolder(allCategoriesFolder, "Northern Ireland",  "categories/northern_ireland/tv");
      AddCategoryFolder(allCategoriesFolder, "Scotland",          "categories/scotland/tv");
      AddCategoryFolder(allCategoriesFolder, "Wales",             "categories/wales/tv");
      AddCategoryFolder(allCategoriesFolder, "Sign Zone",         "categories/signed/tv");
    }

    private void
    AddFolderFromFeed(VirtualFolder parent, string name, string url) {
      VirtualFolder subFolder = new VirtualFolder(CreateGuid(), name, url, true);
      parent.AddFolder(subFolder);
      this.folderLookup[subFolder.Id] = subFolder;
    }

    private void
    AddChannelFolder(VirtualFolder parent, string name, string path) {
      VirtualFolder mainFolder = new VirtualFolder(CreateGuid(), name);
      mainFolder.Thumbnail = "http://www.bbc.co.uk/iplayer/img/station_logos/" + path + ".png";
      parent.AddFolder(mainFolder);
      this.folderLookup[mainFolder.Id] = mainFolder;
      AddFolderFromFeed(mainFolder, "Most popular on " + name, feedRoot + path + "/popular");
      AddFolderFromFeed(mainFolder, name + " highlights",   feedRoot + path + "/highlights");
      AddFolderFromFeed(mainFolder, name + " programmes",   feedRoot + path + "/list");
    }

    private void
    AddCategoryFolder(VirtualFolder parent, string name, string path) {
      VirtualFolder mainFolder = new VirtualFolder(CreateGuid(), name);
      parent.AddFolder(mainFolder);
      this.folderLookup[mainFolder.Id] = mainFolder;
      AddFolderFromFeed(mainFolder, "Most popular " + name, feedRoot + path + "/popular");
      AddFolderFromFeed(mainFolder, name + " highlights",   feedRoot + path + "/highlights");
      AddFolderFromFeed(mainFolder, name + " programmes",   feedRoot + path + "/list");
    }

    private VideoResource
    InfoResource(VirtualFolder parent, string title) {
      return new VideoResource(CreateGuid(), parent.Id, title, "", null,
                               null, DateTime.MinValue, CreateGuid(), null,
                               0, 0, null, null);
    }

    private void
    LoadDynamicFolder(VirtualFolder vf) {
      this.Log("LoadDynamicFolder: "+vf.SourceUrl);

      vf.Reset(); // Remove existing items

      try {
        foreach (ProgrammeItem prog in progDB.ProgrammesFromFeed(vf.SourceUrl)) {
          try {
            string guid = vf.FindGuid(prog.Vpid);
            if (guid == null) guid = CreateGuid();

            VideoResource info =
              new VideoResource(guid, vf.Id, prog.Title, prog.Vpid, prog.Description,
                                prog.Thumbnail, prog.Date, prog.Vpid, null,
                                prog.Duration * 1000 /* PlayOn wants ms */, 0, null, null);

            this.titleLookup[info.Id] = info;
            vf.AddMedia(info);
          } catch (Exception ex) {
            vf.AddMedia(InfoResource(vf, "Error retrieving programme information."));
            this.Log("Error: " + ex);
          }
        }
      } catch (Exception ex) {
        vf.AddMedia(InfoResource(vf, "Error retrieving feed."));
        this.Log("Error: " + ex);
      }
    }

    private string
    CreateGuid() {
      return this.ID + "-" + Guid.NewGuid();
    }

    private List<AbstractSharedMediaInfo>
    GetRange(List<AbstractSharedMediaInfo> list, int startIndex, int requestCount) {
      if (requestCount == 0) {
        requestCount = int.MaxValue;
      }
      if (startIndex > list.Count) {
        return new List<AbstractSharedMediaInfo>(0);
      }
      return list.GetRange(startIndex, Math.Min(requestCount, list.Count - startIndex));
    }

    private void
    Log(string message) {
      this.host.LogMessage(message);
    }
  }
}

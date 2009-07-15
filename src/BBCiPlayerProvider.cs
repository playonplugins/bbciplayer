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
    private string             feedRoot               = "http://feeds.bbc.co.uk/iplayer";

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

      FolderStructure.Folder(rootFolder, folderLookup, feedRoot, delegate(FolderStructure root){
        root.Feed("Most Popular TV", "popular/tv/list");
        root.Feed("TV Highlights",   "highlights/tv");
        root.Folder("TV Channels", delegate(FolderStructure channels){
          channels.Channel("BBC One",          "bbc_one");
          channels.Channel("BBC Two",          "bbc_two");
          channels.Channel("BBC Three",        "bbc_three");
          channels.Channel("BBC Four",         "bbc_four");
          channels.Channel("CBBC",             "cbbc");
          channels.Channel("CBeebies",         "cbeebies");
          channels.Channel("BBC News Channel", "bbc_news24");
          channels.Channel("BBC Parliament",   "bbc_parliament");
          channels.Channel("BBC Alba",         "bbc_alba");
        });
        root.Folder("TV Categories", "categories", delegate(FolderStructure categories){
          categories.Category("Children's",        "childrens/tv");
          categories.Category("Comedy",            "comedy/tv");
          categories.Folder("Drama", "drama", delegate(FolderStructure drama){
            drama.Feed("Most popular Drama",      "popular");
            drama.Feed("Drama highlights",        "highlights");
            drama.Feed("All Drama programmes",    "list");
            drama.Feed("Action & Adventure",      "action_and_adventure/tv/list");
            drama.Feed("Biographical",            "biographical/tv/list");
            drama.Feed("Classic & Period",        "classic_and_period/tv/list");
            drama.Feed("Crime",                   "crime/tv/list");
            drama.Feed("Historical",              "historical/tv/list");
            drama.Feed("Horror & Supernatual",    "horror_and_supernatural/tv/list");
            drama.Feed("Medical",                 "medical/tv/list");
            drama.Feed("Musical",                 "musical/tv/list");
            drama.Feed("Relationships & Romance", "relationship_and_romance/tv/list");
            drama.Feed("SciFi & Fantasy",         "scifi_and_fantasy/tv/list");
            drama.Feed("Soaps",                   "soaps/tv/list");
            drama.Feed("Thriller",                "thriller/tv/list");
          });
          categories.Category("Entertainment",     "entertainment/tv");
          categories.Folder("Factual", "factual", delegate(FolderStructure factual){
            factual.Feed("Most popular Factual",       "tv/popular");
            factual.Feed("Factual highlights",         "tv/highlights");
            factual.Feed("All Factual programmes",     "tv/list");
            factual.Feed("Antiques",                   "antiques/tv/list");
            factual.Feed("Arts, Culture & the Media",  "arts_culture_and_the_media/tv/list");
            factual.Feed("Beauty & Style",             "beauty_and_style/tv/list");
            factual.Feed("Cars & Motors",              "cars_and_motors/tv/list");
            factual.Feed("Cinema",                     "cinema/tv/list");
            factual.Feed("Consumer",                   "consumer/tv/list");
            factual.Feed("Crime & Justice",            "crime_and_justice/tv/list");
            factual.Feed("Disability",                 "disability/tv/list");
            factual.Feed("Families & Relationships",   "families_and_relationships/tv/list");
            factual.Feed("Food & Drink",               "food_and_drink/tv/list");
            factual.Feed("Health & Wellbeing",         "health_and_wellbeing/tv/list");
            factual.Feed("History",                    "history/tv/list");
            factual.Feed("Homes & Gardens",            "homes_and_gardens/tv/list");
            factual.Feed("Life Stories",               "life_stories/tv/list");
            factual.Feed("Money",                      "money/tv/list");
            factual.Feed("Pets & Animals",             "pets_and_animals/tv/list");
            factual.Feed("Politics",                   "politics/tv/list");
            factual.Feed("Football",                   "football/tv/list");
            factual.Feed("Gaelic Games",               "gaelic_games/tv/list");
            factual.Feed("Golf",                       "golf/tv/list");
            factual.Feed("Motorsport",                 "motorsport/tv/list");
            factual.Feed("Rugby League",               "rugby_league/tv/list");
            factual.Feed("Rugby Union",                "regby_union/tv/list");
            factual.Feed("Tennis",                     "tennis/tv/list");
          });
          categories.Category("Films",             "films/tv");
          categories.Folder("Learning", "learning", delegate(FolderStructure learning){
            learning.Feed("Most popular Learning",   "popular");
            learning.Feed("Learning highlights",     "highlights");
            learning.Feed("All Learning programmes", "list");
            learning.Feed("Adult",                   "adult/tv/list");
            learning.Feed("Pre-School",              "pre_school/tv/list");
          });
          categories.Category("Music",             "Music/tv");
          categories.Category("Religion & Ethics", "religion_and_ethics/tv");
          categories.Folder("Sport", "sport", delegate(FolderStructure sport){
            sport.Feed("Most popular Sport",   "tv/popular");
            sport.Feed("Sport highlights",     "tv/highlights");
            sport.Feed("All Sport programmes", "tv/list");
            sport.Feed("Boxing",               "boxing/tv/list");
            sport.Feed("Cricket",              "cricket/tv/list");
            sport.Feed("Equestrian",           "equestrian/tv/list");
            sport.Feed("Football",             "football/tv/list");
            sport.Feed("Gaelic Games",         "gaelic_games/tv/list");
            sport.Feed("Golf",                 "golf/tv/list");
            sport.Feed("Motorsport",           "motorsport/tv/list");
            sport.Feed("Rugby League",         "rugby_league/tv/list");
            sport.Feed("Rugby Union",          "regby_union/tv/list");
            sport.Feed("Tennis",               "tennis/tv/list");
          });
          categories.Category("Northern Ireland",  "northern_ireland/tv");
          categories.Category("Scotland",          "scotland/tv");
          categories.Category("Wales",             "wales/tv");
          categories.Category("Sign Zone",         "signed/tv");
          categories.Feed("Films", "films/tv/list");
        });
      });
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
            vf.AddMedia(InfoResource(vf, "Error retrieving programme information. Please restart PlayOn and check your internet connection."));
            this.Log("Error: " + ex);
          }
        }
      } catch (Exception ex) {
        vf.AddMedia(InfoResource(vf, "Error retrieving feed. Please restart PlayOn and check your internet connection."));
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

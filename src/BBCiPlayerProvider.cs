namespace BBCiPlayer {

  using System;
  using System.Collections;
  using System.Collections.Specialized;
  using System.IO;
  using System.Net;
  using System.Xml;
  using MediaMallTechnologies.Plugin;

  public class BBCiPlayerProvider : IPlayOnProvider {

    private IPlayOnHost    host;
    private VirtualFolder  rootFolder;
    private Hashtable      titleLookup            = new Hashtable();
    private Hashtable      folderLookup           = new Hashtable();
    private int            dynamicFolderCacheTime = 300; // seconds
    private string         magicVPID              = "b0010bg0";

    ////

    public
    BBCiPlayerProvider() {
      this.rootFolder = new VirtualFolder(this.ID, this.Name);
      VirtualFolder subFolder =
        new VirtualFolder(createGuid(),
                          "Popular",
                          "http://feeds.bbc.co.uk/iplayer/popular/tv/list",
                          true);
      this.rootFolder.AddFolder(subFolder);
      this.folderLookup[subFolder.Id] = subFolder;

      subFolder =
        new VirtualFolder(createGuid(),
                          "Hardcoded");
      AddHardCodedTitle(subFolder);
      this.rootFolder.AddFolder(subFolder);
      this.folderLookup[subFolder.Id] = subFolder;
    }

    ////

    public string
    Name {
      get { return "BBC iPlayer"; }
    }

    public string
    ID {
      get { return this.Name.Replace(" ", "").ToLower(); }
    }

    public Payload
    GetSharedMedia(string id, bool includeChildren, int startIndex, int requestCount) {
      this.Log("GetSharedMedia");

      if (id == null || id.Length == 0) {
        return new Payload("-1", "-1", "[Invalid Request]", 0, new ArrayList(0));
      }

      ArrayList currentList = new ArrayList();

      if (id == this.ID) { // root
        foreach (VirtualFolder subFolder in this.rootFolder.Items) {
          subFolder.ParentId = this.ID;
          currentList.Add(new SharedMediaFolderInfo(subFolder.Id, id, subFolder.Title, subFolder.Items.Count));
        }
        return new Payload(id, "0", this.Name, currentList.Count, getRange(currentList, startIndex, requestCount));
      }

      if (titleLookup[id] != null) {
        SharedMediaFileInfo fileInfo = (SharedMediaFileInfo)titleLookup[id];
        currentList.Add(fileInfo);
        return new Payload(id, fileInfo.OwnerId, fileInfo.Title, 1, currentList, false);
      }

      if (folderLookup[id] != null) {
        VirtualFolder folder = (VirtualFolder)folderLookup[id];

        if (folder.Dynamic && (DateTime.Now - folder.LastLoad).TotalSeconds > this.dynamicFolderCacheTime) {
          loadDynamicFolder(folder);
          folder.LastLoad = DateTime.Now;
        }

        foreach (object entry in folder.Items) {
          if (entry is VirtualFolder) {
            VirtualFolder subFolder = entry as VirtualFolder;
            currentList.Add(
              new SharedMediaFolderInfo(subFolder.Id, subFolder.Id, subFolder.Title, subFolder.Items.Count));
          } else if (entry is SharedMediaFileInfo) {
            SharedMediaFileInfo fileInfo = (SharedMediaFileInfo)entry;
            currentList.Add(fileInfo);
          }
        }
        return new Payload(id, folder.ParentId, folder.Title, currentList.Count,
                           getRange(currentList, startIndex, requestCount));
      }

      return new Payload("-1", "-1", "[Unknown Request]", 0, new ArrayList(0));
    }

    public System.Drawing.Image
    Image {
      get {
        System.Drawing.Image image = null;
        Stream imageStream = System.Reflection.Assembly.GetExecutingAssembly().
                             GetManifestResourceStream("Logo48x48.png");
        if (imageStream != null) {
          image = System.Drawing.Image.FromStream(imageStream);
          imageStream.Close();
        }
        return image;
      }
    }

    public string
    Resolve(SharedMediaFileInfo fileInfo) {
      this.Log("Resolve: " + fileInfo.Path);

      string type = "fp";
      string url = StreamingURLForVPID(fileInfo.Path);
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
    }

    ////

    private string
    readFromURL(string url) {
      this.Log("readFromURL: "+url);

      HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
      StreamReader sr    = new StreamReader(req.GetResponse().GetResponseStream());
      string content     = sr.ReadToEnd();
      sr.Close();
      return content;
    }

    private void
    loadDynamicFolder(VirtualFolder vf) {
      this.Log("loadDynamicFolder: "+vf.SourceURL);

      try {
        vf.Reset(); // Remove existing items

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(readFromURL(vf.SourceURL));

        XmlNamespaceManager atomNS = new XmlNamespaceManager(doc.NameTable);
        atomNS.AddNamespace("atom", "http://www.w3.org/2005/Atom");
        atomNS.AddNamespace("media", "http://search.yahoo.com/mrss/");

        foreach (XmlNode entry in doc.GetElementsByTagName("entry")) {
          string title =
            entry.SelectSingleNode("atom:title", atomNS).InnerText;
          string description =
            "";
          string url =
            entry.SelectSingleNode("atom:link[@rel='alternate']", atomNS).Attributes["href"].Value;
          string thumbnail =
            entry.SelectSingleNode("atom:link/media:content/media:thumbnail", atomNS).Attributes["url"].Value;
          DateTime date =
            DateTime.Parse(
              entry.SelectSingleNode("atom:updated", atomNS).InnerText,
              System.Globalization.CultureInfo.InvariantCulture);
          long duration = 0;

          string guid = vf.FindGuid(url);
          if (guid == null) guid = createGuid();

          VideoResource info =
            new VideoResource(guid, vf.Id, title, url, description, thumbnail, date, url, null,
                              duration, 0, null, null);

          this.titleLookup[info.Id] = info;
          vf.AddMedia(info);
        }
      } catch (Exception ex) {
        this.Log("Error: " + ex);
      }
    }

    private void
    AddHardCodedTitle(VirtualFolder vf) {
      string   title       = "HARDCODED";
      string   url         = this.magicVPID;
      string   description = "Try me out";
      string   thumbnail   = null;
      DateTime date        = DateTime.MinValue;
      long     duration    = 0;

      string guid = vf.FindGuid(url);
      if (guid == null) guid = createGuid();

      VideoResource info =
        new VideoResource(guid, vf.Id, title, url, description, thumbnail, date, url, null,
                          duration, 0, null, null);

      this.titleLookup[info.Id] = info;
      vf.AddMedia(info);
    }

    private string
    StreamingURLForVPID(string vpid) {
      string url = "http://www.bbc.co.uk/mediaselector/4/mtis/stream/" + vpid;

      XmlDocument doc = new XmlDocument();
      doc.LoadXml(readFromURL(url));

      XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
      ns.AddNamespace("bbc", "http://bbc.co.uk/2008/mp/mediaselection");

      XmlNode entry     = doc.SelectSingleNode("//bbc:media[@encoding='vp6']/bbc:connection", ns);
      string server     = entry.Attributes["server"].Value;
      string authString = entry.Attributes["authString"].Value;
      string identifier = entry.Attributes["identifier"].Value;

      return "rtmp://" + server + "/ondemand?_fcs_vhost=" + server +
             "&auth=" + authString + "&aifp=v001&slist=" + identifier + "|" +
             "<rtmpMedia version=\"1.0\"><mediaPath>" + identifier + "</mediaPath></rtmpMedia>";
    }

    private string
    createGuid() {
      return this.ID + "-" + Guid.NewGuid();
    }

    private ArrayList
    getRange(ArrayList list, int startIndex, int requestCount) {
      if (requestCount == 0) {
        requestCount = int.MaxValue;
      }
      if (startIndex > list.Count) {
        return new ArrayList(0);
      }
      return list.GetRange(startIndex, Math.Min(requestCount, list.Count - startIndex));
    }

    private void
    Log(string message) {
      this.host.LogMessage(message);
    }
  }
}

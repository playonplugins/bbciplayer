namespace Beeb {

  using System;
  using System.IO;
  using System.Net;
  using System.Xml;

  public class ProgrammeDatabase {

    private LruCache vpidToStreamingUrlCache   = new LruCache(200);
    private LruCache programmeInformationCache = new LruCache(200);

    ////

    public
    ProgrammeDatabase() {}

    ////

    public ProgrammeItem
    ProgrammeInformation(string pid) {
      ProgrammeItem programmeItem = (ProgrammeItem)this.programmeInformationCache.Get(pid);

      if (programmeItem == null) {
        programmeItem = RemoteLookUpProgrammeItem(pid);
        this.programmeInformationCache.Set(pid, programmeItem);
      }

      return programmeItem;
    }

    public string
    VpidToStreamingUrl(string vpid) {
      string streamingUrl = (string)this.vpidToStreamingUrlCache.Get(vpid);

      if (streamingUrl == null) {
        streamingUrl = RemoteLookUpStreamingUrl(vpid);
        this.vpidToStreamingUrlCache.Set(vpid, streamingUrl);
      }

      return streamingUrl;
    }

    ////

    private string
    ReadFromUrl(string url) {
      HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
      StreamReader sr    = new StreamReader(req.GetResponse().GetResponseStream());
      string content     = sr.ReadToEnd();
      sr.Close();
      return content;
    }

    private string
    RemoteLookUpStreamingUrl(string vpid) {
      XmlDocument doc = new XmlDocument();
      string mediaSelectorUrl = "http://www.bbc.co.uk/mediaselector/4/mtis/stream/" + vpid;
      doc.LoadXml(ReadFromUrl(mediaSelectorUrl));

      XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
      ns.AddNamespace("bbc", "http://bbc.co.uk/2008/mp/mediaselection");

      XmlNode entry     = doc.SelectSingleNode("//bbc:media[@encoding='vp6']/bbc:connection", ns);
      if (entry == null) return null;

      string server     = entry.Attributes["server"].Value;
      string authString = entry.Attributes["authString"].Value;
      string identifier = entry.Attributes["identifier"].Value;

      return "rtmp://" + server + "/ondemand?_fcs_vhost=" + server +
             "&auth=" + authString + "&aifp=v001&slist=" + identifier + "|" +
             "<rtmpMedia version=\"1.0\"><mediaPath>" + identifier + "</mediaPath></rtmpMedia>";
    }

    private ProgrammeItem
    RemoteLookUpProgrammeItem(string pid) {
      string playlistUrl = "http://www.bbc.co.uk/iplayer/playlist/" + pid;

      XmlDocument doc = new XmlDocument();
      doc.LoadXml(ReadFromUrl(playlistUrl));

      XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
      ns.AddNamespace("pl", "http://bbc.co.uk/2008/emp/playlist");

      XmlNode item = doc.SelectSingleNode("//pl:item[@kind='programme'][descendant::pl:alternate[@id='default']]", ns);
      if (item == null) return null;

      string duration   = item.Attributes["duration"].Value;
      string identifier = item.Attributes["identifier"].Value;

      return new ProgrammeItem(identifier, System.Int64.Parse(duration));
    }
  }
}

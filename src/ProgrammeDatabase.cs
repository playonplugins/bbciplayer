namespace Beeb {

  using System;
  using System.IO;
  using System.Xml;
  using System.Collections.Generic;
  using System.Text.RegularExpressions;

  public class ProgrammeDatabase {

    private LruCache programmeInformationCache = new LruCache(200);

    ////

    public string
    VpidToStreamingUrl(string vpid) {
      XmlDocument doc = new XmlDocument();
      string mediaSelectorUrl = "http://www.bbc.co.uk/mediaselector/4/mtis/stream/" + vpid;
      doc.LoadXml(Beeb.Util.ReadFromUrl(mediaSelectorUrl));

      XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
      ns.AddNamespace("bbc", "http://bbc.co.uk/2008/mp/mediaselection");

      XmlNode entry = doc.SelectSingleNode("//bbc:media[@service='iplayer_streaming_h264_flv']/bbc:connection", ns);
      if (entry == null) return null;

      string identifier = entry.Attributes["identifier"].Value;
      string swfurl     = "http://www.bbc.co.uk/emp/9player.swf?revision=10344_10753";
      string server     = entry.Attributes["server"].Value;
      string authString = entry.Attributes["authString"].Value;

      switch (entry.Attributes["kind"].Value) {
        case "akamai":
          identifier = Regex.Replace(identifier, @"^mp4:", "");
          break;
        case "limelight":
          break;
        default:
          return null;
      }

      StringWriter sw = new StringWriter();
      XmlTextWriter writer = new XmlTextWriter(sw);

      writer.Formatting = Formatting.Indented;
      writer.WriteStartElement("rtmpMedia");
        writer.WriteAttributeString("version", "1.0");
        writer.WriteStartElement("mediaPath");
          writer.WriteString(identifier);
        writer.WriteEndElement();
        writer.WriteStartElement("swfUrl");
          writer.WriteString(swfurl);
        writer.WriteEndElement();
      writer.WriteEndElement();
      writer.Close();
      string xml = sw.ToString();

      return "rtmp://" + server + "/ondemand?_fcs_vhost=" + server +
             "&auth=" + authString + "&aifp=v001&slist=" + identifier +
             "|" + xml;
    }

    public List<ProgrammeItem>
    ProgrammesFromFeed(string feedUrl) {
      List<ProgrammeItem> items = new List<ProgrammeItem>();

      XmlDocument doc = new XmlDocument();
      doc.LoadXml(Beeb.Util.ReadFromUrl(feedUrl));

      XmlNamespaceManager atomNS = new XmlNamespaceManager(doc.NameTable);
      atomNS.AddNamespace("atom", "http://www.w3.org/2005/Atom");
      atomNS.AddNamespace("media", "http://search.yahoo.com/mrss/");

      foreach (XmlNode entry in doc.GetElementsByTagName("entry")) {
        string url       = entry.SelectSingleNode("atom:link[@rel='alternate']", atomNS).Attributes["href"].Value;
        string pid       = Regex.Match(url, @"/iplayer/episode/([a-z0-9]{8})").Groups[1].Value;

        ProgrammeItem prog = ProgrammeInformation(pid);

        if (prog != null) {
          // The thumbnail is the only piece of information we can't delegate to the playlist:
          prog.Thumbnail = entry.SelectSingleNode("atom:link/media:content/media:thumbnail", atomNS).Attributes["url"].Value/*.
                           Replace("150_84", "640_360")*/;

          items.Add(prog);
        }
      }

      if (FeedShouldBeAlphabetized(feedUrl)) {
        items.Sort(delegate(ProgrammeItem a, ProgrammeItem b) { return a.Title.CompareTo(b.Title); });
      }

      return items;
    }

    ////

    private bool
    FeedShouldBeAlphabetized(string feedUrl) {
      return Regex.Match(feedUrl, @"/list$").Success;
    }

    private ProgrammeItem
    ProgrammeInformation(string pid) {
      ProgrammeItem programmeItem = (ProgrammeItem)this.programmeInformationCache.Get(pid);

      if (programmeItem == null) {
        programmeItem = RemoteLookUpProgrammeItem(pid);
        this.programmeInformationCache.Set(pid, programmeItem);
      }

      return programmeItem;
    }

    private ProgrammeItem
    RemoteLookUpProgrammeItem(string pid) {
      ProgrammeItem prog = new ProgrammeItem();

      string playlistUrl = "http://www.bbc.co.uk/iplayer/playlist/" + pid;

      XmlDocument doc = new XmlDocument();
      doc.LoadXml(Beeb.Util.ReadFromUrl(playlistUrl));

      XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
      ns.AddNamespace("pl", "http://bbc.co.uk/2008/emp/playlist");

      XmlNode item = doc.SelectSingleNode("//pl:item[@kind='programme'][descendant::pl:alternate[@id='default']]", ns);
      if (item == null) {
        item = doc.SelectSingleNode("//pl:item[@kind='programme'][descendant::pl:alternate]", ns);
      }
      if (item == null) return null;

      prog.Title       = doc.SelectSingleNode("pl:playlist/pl:title", ns).InnerText;
      prog.Description = doc.SelectSingleNode("pl:playlist/pl:summary", ns).InnerText;
      prog.Date  = DateTime.Parse( doc.SelectSingleNode("pl:playlist/pl:updated", ns).InnerText,
                                      System.Globalization.CultureInfo.InvariantCulture );

      try {
        prog.Duration = System.Int64.Parse(item.Attributes["duration"].Value);
      } catch {
        prog.Duration = 5 * 60 * 60; // 5 hours
      }
      prog.Vpid     = item.Attributes["identifier"].Value;

      return prog;
    }
  }
}

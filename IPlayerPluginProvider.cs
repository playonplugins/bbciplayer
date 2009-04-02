/*
 *  Copyright (c) 2003-2009 MediaMall Technologies, Inc.
 *  All rights reserved.
 * 
 *  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
 *  EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 *  OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
 *  SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 *  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
 *  OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 *  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
 *  OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 *  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Xml;
using MediaMallTechnologies.Plugin;

namespace IPlayerPlugin {
  public class IPlayerPluginProvider : MediaMallTechnologies.Plugin.IPlayOnProvider {

    private IPlayOnHost host;
    private VirtualFolder rootFolder;
    private Hashtable titleLookup = new Hashtable();
    private Hashtable folderLookup = new Hashtable();

    /* ------------------------------------------------------------- */

    public IPlayerPluginProvider() {

      // create a root folder
      this.rootFolder = new VirtualFolder(this.ID, this.Name);

      // create a sub folder
      VirtualFolder subFolder = new VirtualFolder(createGuid(), "Sample Folder", "http://www.themediamall.com/downloads/playon/plugins/api/sample/sample.xml", true);

      // add this folder and cache its ID for lookups
      this.rootFolder.AddFolder(subFolder);
      this.folderLookup[subFolder.Id] = subFolder;
    }

    /* ------------------------------------------------------------- */

    private void load(VirtualFolder vf) {

      try {
        // reset this folder to clear potential stale content (relevant for dynamically loaded data)
        vf.Reset();

        // load dynamic data
        HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(vf.SourceURL);
        StreamReader sr = new StreamReader(req.GetResponse().GetResponseStream());
        string xml = sr.ReadToEnd();
        sr.Close();

        // parse XML
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml);
        XmlNodeList items = doc.GetElementsByTagName("item");
        foreach (XmlNode node in items) {
          string title = "", duration = "", description = "", date = "", thumbnail = "", url = "";
          foreach (XmlNode child in node.ChildNodes) {
            switch (child.Name) {
              case "title":
                title = child.InnerText;
                break;
              case "description":
                description = child.InnerText;
                break;
              case "url":
                url = child.InnerText;
                break;
              case "thumbnail":
                thumbnail = child.InnerText;
                break;
              case "duration":
                // required format is "H:MM:SS"
                TimeSpan ts = TimeSpan.FromMilliseconds(double.Parse(child.InnerText));
                duration = ts.Hours + ":" + ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2");
                break;
              case "pubDate":
                // required format is "2008-04-10T06:30:00"
                date = DateTime.Parse(child.InnerText, System.Globalization.CultureInfo.InvariantCulture).ToString("s");
                break;
            }
          }

          // add media
          NameValueCollection props = new NameValueCollection();
          props["Duration"] = duration;
          props["Description"] = description;
          props["Date"] = date;
          props["Icon"] = thumbnail;

          // create ID
          string guid = vf.FindGuid(url);
          if (guid == null)
            guid = createGuid();
          SharedOnlineMediaInfo info = new SharedOnlineMediaInfo(guid, vf.Id, title, url, 2, props, url);

          // cache lookup and add to folder
          this.titleLookup[info.Id] = info;
          vf.AddMedia(info);
        }
      }
      catch (Exception ex) {
        log("Error: " + ex);
      }
    }

    /* ------------------------------------------------------------- */

    public string Name {
      get {
        return "Sample Plugin";
      }
    }

    /* ------------------------------------------------------------- */

    public string ID {
      get {
        return this.Name.Replace(" ", "").ToLower();
      }
    }

    /* ------------------------------------------------------------- */

    private string createGuid() {
      return this.ID + "-" + Guid.NewGuid();
    }

    /* ------------------------------------------------------------- */

    private ArrayList getRange(ArrayList list, int startIndex, int requestCount) {

      if (requestCount == 0)
        requestCount = int.MaxValue;
      ArrayList items;
      if (startIndex > list.Count) {
        items = new ArrayList(0);
      }
      else {
        items = list.GetRange(startIndex, Math.Min(requestCount, list.Count - startIndex));
      }
      return items;
    }

    /* ------------------------------------------------------------- */

    public MediaMallTechnologies.Plugin.Payload GetSharedMedia(string id, bool includeChildren, int startIndex, int requestCount) {

      if (id == null || id.Length == 0)
        return new Payload("-1", "-1", "[Unknown]", 0, new ArrayList(0));

      ArrayList currentList;

      // Root
      if (id == this.ID) {
        // return all subfolders for this root folder
        currentList = new ArrayList();
        foreach (VirtualFolder vf in this.rootFolder.Items) {
          vf.ParentId = this.ID;
          currentList.Add(new SharedMediaFolderInfo(vf.Id, id, vf.Title, vf.Items.Count));
        }
        return new Payload(id, "0", this.Name, currentList.Count, getRange(currentList, startIndex, requestCount));
      }
      else {
        // if ID is for an item, return it
        if (titleLookup[id] != null) {
          SharedOnlineMediaInfo fileInfo = (SharedOnlineMediaInfo)titleLookup[id];
          currentList = new ArrayList();
          currentList.Add(fileInfo);
          return new Payload(id, fileInfo.OwnerId, fileInfo.Title, 1, currentList, false);
        }
        // if ID is for a folder, return it with subfolders and items
        if (folderLookup[id] != null) {
          currentList = new ArrayList();
          VirtualFolder vf = (VirtualFolder)folderLookup[id];
          
          // load this folder if dynamic (but avoid unnecessary web traffic)
          if (vf.Dynamic && (DateTime.Now - vf.LastLoad).TotalSeconds > 300) {
            load(vf);
            vf.LastLoad = DateTime.Now;
          }

          foreach (object o in vf.Items) {
            if (o is VirtualFolder) {
              VirtualFolder folder = o as VirtualFolder;
              currentList.Add(new SharedMediaFolderInfo(folder.Id, vf.Id, folder.Title, folder.Items.Count));
            }
            else if (o is SharedOnlineMediaInfo) {
              SharedOnlineMediaInfo file = o as SharedOnlineMediaInfo;
              currentList.Add(file);
            }
          }
          return new Payload(id, vf.ParentId, vf.Title, currentList.Count, getRange(currentList, startIndex, requestCount));
        }

        return new Payload("-1", "-1", "[Unknown]", 0, new ArrayList(0));
      }
    }

    /* ------------------------------------------------------------- */

    public System.Drawing.Image Image {
      get {
        System.Drawing.Image image = null;
        Stream imageStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("IPlayerPlugin.Sample.png");
        if (imageStream != null) {
          image = System.Drawing.Image.FromStream(imageStream);
          imageStream.Close();
        }
        return image;
      }
    }

    /* ------------------------------------------------------------- */

    public string Resolve(MediaMallTechnologies.Plugin.SharedMediaFileInfo fileInfo) {
      string type = fileInfo.Path.EndsWith(".wmv") ? "wmp" : "fp";
      string xml = "<media><url type=\"" + type + "\">" + fileInfo.Path + "</url></media>";
      return xml;
    }

    /* ------------------------------------------------------------- */

    public void SetPlayOnHost(IPlayOnHost h) {
      this.host = h;
    }

    /* ------------------------------------------------------------- */

    private void log(string message) {
      this.host.LogMessage(message);
    }

  }
}

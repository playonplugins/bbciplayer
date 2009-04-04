using System.Collections.Specialized;
using MediaMallTechnologies.Plugin;

namespace IPlayerPlugin {
  public class SharedOnlineMediaInfo : SharedMediaFileInfo {

    private string onlineIdentifier;

    public
    SharedOnlineMediaInfo(
      string id, string ownerId, string title, string path, int type,
      NameValueCollection props, string onlineIdentifier)
      : base(id, ownerId, title, path, type, props)
    {
      this.onlineIdentifier = onlineIdentifier;
    }

    public string
    OnlineIdentifier {
      get { return this.onlineIdentifier; }
      set { this.onlineIdentifier = value; }
    }
  }
}

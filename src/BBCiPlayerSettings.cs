namespace Beeb {

  using System;
  using System.Collections.Specialized;
  using System.Drawing;
  using System.IO;
  using System.Net;
  using System.Reflection;
  using System.Windows.Forms;

  public class BBCiPlayerSettings : MediaMallTechnologies.Plugin.IPlayOnProviderSettings {

    public System.Drawing.Image
    Image {
      get {
        Image image = null;
        Stream imageStream = System.Reflection.Assembly.GetExecutingAssembly().
                             GetManifestResourceStream("Logo78x78.png");
        if (imageStream != null) {
          image = System.Drawing.Image.FromStream(imageStream);
          imageStream.Close();
        }
        return image;
      }
    }

    public string
    Link {
      get { return "http://www.bbc.co.uk/iplayer"; }
    }

    public string
    Name {
      get { return "BBC iPlayer"; }
    }

    public string
    Description {
      get { return "Watch BBC programmes."; }
    }

    public string
    ID {
      get { return "a978d03a-08b9-44db-89af-c7bd976c8747"; }
    }

    public bool
    RequiresLogin {
      get { return false; }
    }

    public bool
    HasOptions {
      get { return false; }
    }

    public bool
    TestLogin(string username, string password) {
      return true;
    }

    public string
    CheckForUpdate() {
      return null;
    }

    public System.Windows.Forms.Control
    ConfigureOptions(NameValueCollection props, EventHandler e) {
      return null;
    }

  }
}

namespace Beeb {

  using System.Net;
  using System.IO;

  class Util {
    public static string
    ReadFromUrl(string url) {
      for (int triesLeft = 3; triesLeft > 0; triesLeft--) {
        try {
          HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
          StreamReader sr    = new StreamReader(req.GetResponse().GetResponseStream());
          string content     = sr.ReadToEnd();
          sr.Close();
          return content.Trim();
        } catch(System.Net.WebException ex) {
          if (triesLeft == 0) throw ex;
        }
      }
      return null; // Unreachable, but compiler doesn't realise.
    }
  }
}

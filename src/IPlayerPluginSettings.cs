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
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;

namespace IPlayerPlugin {

  public class IPlayerPluginSettings : MediaMallTechnologies.Plugin.IPlayOnProviderSettings {

    public System.Drawing.Image Image {
      get {
        Image image = null;
        Stream imageStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Logo48x48.png");
        if (imageStream != null) {
          image = System.Drawing.Image.FromStream(imageStream);
          imageStream.Close();
        }
        return image;
      }
    }

    public string Link {
      get {
        return "http://www.bbc.co.uk/iplayer";
      }
    }

    public string Name {
      get {
        return "BBC iPlayer";
      }
    }

    public string Description {
      get {
        return "Watch and listen to BBC programmes.";
      }
    }

    public bool RequiresLogin {
      get {
        return false;
      }
    }

    public bool TestLogin(string username, string password) {
      return true;
    }

    public string CheckForUpdate() {
      return null;
    }

    public NameValueCollection ConfigureOptions() {
      return null;
    }

  }
}

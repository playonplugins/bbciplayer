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

namespace IPlayerPlugin {
  public class VirtualFolder {

    // ---------------------------------------------------------------
    // Instance fields
    // ---------------------------------------------------------------

    private string title;
    private string id;
    private string parentId;
    private string sourceUrl;
    private bool dynamic = false;
    private ArrayList items = new ArrayList();
    private Hashtable lookup = new Hashtable();
    private string filter;
    private string identifier = null;
    private DateTime lastLoad = DateTime.MinValue;

    // ---------------------------------------------------------------
    // Constructors
    // ---------------------------------------------------------------

    public VirtualFolder(string id, string title)
      : this(id, title, "", false) {
    }

    /* ------------------------------------------------------------- */

    public VirtualFolder(string id, string title, string sourceUrl)
      : this(id, title, sourceUrl, false) {
    }

    /* ------------------------------------------------------------- */

    public VirtualFolder(string id, string title, string sourceUrl, bool dynamic) {
      this.id = id;
      this.title = title;
      this.sourceUrl = sourceUrl;
      this.dynamic = dynamic;
      this.parentId = "-1";
    }

    // ---------------------------------------------------------------
    // Instance methods
    // ---------------------------------------------------------------

    public string Title {
      get {
        return this.title;
      }
      set {
        this.title = value;
      }
    }

    /* ------------------------------------------------------------- */

    public string Id {
      get {
        return this.id;
      }
      set {
        this.id = value;
      }
    }

    /* ------------------------------------------------------------- */

    public string ParentId {
      get {
        return this.parentId;
      }
      set {
        this.parentId = value;
      }
    }

    /* ------------------------------------------------------------- */

    public ArrayList Items {
      get {
        return this.items;
      }
    }

    /* ------------------------------------------------------------- */

    public string Filter {
      get {
        return this.filter;
      }
      set {
        this.filter = value;
      }
    }

    /* ------------------------------------------------------------- */

    public string Identifier {
      get {
        return this.identifier;
      }
      set {
        this.identifier = value;
      }
    }

    /* ------------------------------------------------------------- */

    public DateTime LastLoad {
      get {
        return this.lastLoad;
      }
      set {
        this.lastLoad = value;
      }
    }

    /* ------------------------------------------------------------- */

    public void Reset() {
      this.items.Clear();
    }

    /* ------------------------------------------------------------- */

    public void AddMedia(SharedOnlineMediaInfo info) {
      this.items.Add(info);
      info.OwnerId = this.id;
      this.lookup[info.OnlineIdentifier] = info.Id;
    }

    /* ------------------------------------------------------------- */

    public void AddFolder(VirtualFolder folder) {
      this.items.Add(folder);
      folder.ParentId = this.id;
      if (folder.Filter == null || folder.Filter.Length == 0)
        folder.Filter = this.filter;
      if (folder.Identifier != null)
        this.lookup[folder.Identifier] = folder.Id;
    }

    /* ------------------------------------------------------------- */

    public string FindGuid(string onlineIdentifier) {
      return lookup[onlineIdentifier] as string;
    }

    /* ------------------------------------------------------------- */

    public string SourceURL {
      get {
        return this.sourceUrl;
      }
      set {
        this.sourceUrl = value;
      }
    }

    /* ------------------------------------------------------------- */

    public bool Dynamic {
      get {
        return this.dynamic;
      }
      set {
        this.dynamic = value;
      }
    }
  }

}

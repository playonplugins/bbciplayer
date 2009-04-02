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
using System.Collections.Specialized;
using MediaMallTechnologies.Plugin;

namespace IPlayerPlugin {

  public class SharedOnlineMediaInfo : SharedMediaFileInfo {

    // ---------------------------------------------------------------
    // Instance fields
    // ---------------------------------------------------------------

    private string onlineIdentifier;

    // ---------------------------------------------------------------
    // Constructors
    // ---------------------------------------------------------------

    public SharedOnlineMediaInfo(string id, string ownerId, string title, string path, int type, NameValueCollection props, string onlineIdentifier)
      : base(id, ownerId, title, path, type, props) {
      this.onlineIdentifier = onlineIdentifier;
    }

    // ---------------------------------------------------------------
    // Instance methods
    // ---------------------------------------------------------------

    public string OnlineIdentifier {
      get {
        return this.onlineIdentifier;
      }
      set {
        this.onlineIdentifier = value;
      }
    }
  }
}

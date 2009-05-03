namespace Beeb {
  public class ProgrammeItem {

    private string vpid;
    private long   duration;

    ////

    public
    ProgrammeItem(string vpid, long duration) {
      this.vpid     = vpid;
      this.duration = duration;
    }

    ////

    public string
    Vpid {
      get { return this.vpid; }
    }

    public long
    Duration {
      get { return this.duration; }
    }
  }
}

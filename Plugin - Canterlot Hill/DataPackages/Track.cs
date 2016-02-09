using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Canterlot_Hill {
    public class CanterlotTrack : Plugin_Base.Track {
        #region Track Interface
        public string TrackID { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public int DurationSec { get; set; }
        public int Rating { get; set; }
        #endregion

        public String WholeTitle { get { return (!String.IsNullOrEmpty(Artist) ? Artist.Trim() + " - " : "") + Title.Trim(); } }
        public String Plays { get; set; }
        public String Requests { get; set; }
        public String LastPlayed { get; set; } //THIS IS UTC TIME IF GOT FROM getTrack
        public DateTime LocalLastPlayed {
            get {
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(LastPlayed), TimeZoneInfo.Local);
            }
        }

        public int requestable { get; set; }
        public double RequestTime { get; set; }
        public int forced { get; set; }
        public String Requester { get; set; }
        public int Priority { get; set; }

        public static CanterlotTrack Parse(String s) {
            return JsonConvert.DeserializeObject(s) as CanterlotTrack;
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }

        public String GetLastPlayed() {
            return DateTime.Parse(LastPlayed).ToString("mm:ss");
        }
    }
}

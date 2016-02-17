using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin_Base;

namespace Plugin {
    public class CanterlotTrack : Plugin_Base.Track {
        #region Track Interface
        public String TrackID { get; set; }
        public String Title { get; set; }
        public String Artist { get; set; }
        public String Requester { get; set; }
        public int DurationSec { get; set; }
        public int Rating { get; set; }
        public int Plays { get; set; }
        public int Requests { get; set; }
        public DateTime LastPlayed { get; set; }
        public DateTime RequestedTime { get; set; }
        #endregion

        public DateTime LocalLastPlayed {
            get {
                return TimeZoneInfo.ConvertTimeFromUtc(LastPlayed, TimeZoneInfo.Local);
            }
        }

        public static CanterlotTrack Parse(String s) {
            return JsonConvert.DeserializeObject(s) as CanterlotTrack;
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }
}

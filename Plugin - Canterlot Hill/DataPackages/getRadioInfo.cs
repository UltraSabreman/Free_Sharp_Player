using Newtonsoft.Json;
using System;

namespace Plugin {
    public class GetRadioInfo {
        public bool up;
        public String listeners;
        public String title;
        public String rating;
        public String url;
        public ServList servers = new ServList();
        public String track_id;
        public String autoDJ;



        public class ServList {
            [JsonProperty("80 KBPS")]
            public String lowQuality { get; set; }
            [JsonProperty("128 KBPS")]
            public String medQuality { get; set; }
            [JsonProperty("320 KBPS")]
            public String highQuality { get; set; }
        }
    }
	
}

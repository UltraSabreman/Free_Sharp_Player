using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin_Base.DataPackages;

namespace Plugin_Canterlot_Hill {
	public class GetRadioInfo : BaseDataPackage {
        public bool up;
        public String listeners;
        public String title;
        public String rating;
        public String url;
		public ServList servers;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugin_Base.DataPackages;

namespace Plugin_Canterlot_Hill {
	//The search feature.
	public class GetTracks : BaseDataPackage {
		public String total_records;
		public int pages;
		public List<CanterlotTrack> track;
		public Vars variables;

		public class Vars {
			public String artist;
			public String Track;
			public int page;
			public int limit;
			public String rating;
			public String rating_direction;
		}

	}
}

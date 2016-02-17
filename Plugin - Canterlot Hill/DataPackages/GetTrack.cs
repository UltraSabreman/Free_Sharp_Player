using System;
using System.Collections.Generic;

namespace Plugin {
    //The search feature.
    public class GetTracks {
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

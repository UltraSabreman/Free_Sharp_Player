using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	class PlaylistData {
		public List<Track> Played { get; set; }
		public List<Track> Queue { get; set; }
		public Track Playing { get; set; }
	}
}

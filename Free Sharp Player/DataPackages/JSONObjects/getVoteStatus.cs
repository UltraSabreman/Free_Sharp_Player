using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	class getVoteStatus : JSONBase {
		public Data data;

		public class Data : ViewModelNotifier {
			int status;
		}
	}
}

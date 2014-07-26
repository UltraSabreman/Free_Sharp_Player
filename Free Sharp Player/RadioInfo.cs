using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	class RadioInfo: ViewModelNotifier {
		public bool up;
		public int listeners { get { return GetProp<int>(); } set { SetProp(value); } }
		public String title;
		public int? rating { get { return GetProp<int>(); } set { SetProp(value); } }

		public bool autoDJ;
		public int? trackID;
	}
}

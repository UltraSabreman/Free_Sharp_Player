﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	class setVote : JSONBase {
		public Data data;

		public class Data : ViewModelNotifier {
			public int status;
			public int vote;
			public int inserted;
			public int updated;
			public int deleted;
		}
	}
}

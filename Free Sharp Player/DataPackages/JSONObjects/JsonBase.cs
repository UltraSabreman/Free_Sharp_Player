using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	class JSONBase : ViewModelNotifier {
		public String action { get; set; }
		public int? valid { get; set; }
		public Error error;


		public class Error {
			public int code { get; set; }
			public String message { get; set; }
		}
	}
}

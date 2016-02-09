using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin_Base.DataPackages;

namespace Plugin_Canterlot_Hill {
	class SetVote : BaseDataPackage {
		public int status;
		public int vote;
		public int inserted;
		public int updated;
		public int deleted;
    }
}

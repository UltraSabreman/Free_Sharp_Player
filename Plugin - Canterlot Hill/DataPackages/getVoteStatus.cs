using Newtonsoft.Json;
using Plugin_Base.DataPackages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Canterlot_Hill {
	class GetVoteStatus : BaseDataPackage {
		public int? vote; //my vote.
		public int? status; //if here i can vote
	}
}

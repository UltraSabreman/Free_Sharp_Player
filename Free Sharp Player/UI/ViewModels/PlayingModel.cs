using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	public class PlayingModel : ViewModelNotifier {
		public String StreamTitle { get { return GetProp<String>(); } set { SetProp(value); } }
		public int PosInBuffer { get { return GetProp<int>(); } set { SetProp(value); } }
		public int BufferLen { get { return GetProp<int>(); } set { SetProp(value); } }

		private MainWindow window;

		public PlayingModel(MainWindow win) {
			window = win;

			window.bar_Buffer.DataContext = this;
			window.bar_BufferWindow.DataContext = this;
			window.lbl_TrackName.DataContext = this;
		}

	}
}

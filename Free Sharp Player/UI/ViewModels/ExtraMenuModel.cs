using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Free_Sharp_Player {
	class ExtraMenuModel : ViewModelNotifier {
		public int Votes { get { return GetProp<int>(); } set { SetProp(value); } }

		private MainWindow window;

		public ExtraMenuModel(MainWindow win) {
			window = win;

			window.Extras.DataContext = this;

			window.btn_Like.Click += ResetCapture;
			window.btn_Dislike.Click += ResetCapture;
			window.btn_Favor.Click += ResetCapture;
			window.btn_Request.Click += ResetCapture;
			window.btn_Settings.Click += ResetCapture;

		}

		public void Tick(Object o, EventArgs e) {

		}

		private void ResetCapture(object o, object e) {
			Mouse.Capture(window.Extras, CaptureMode.SubTree);
		}
	}
}

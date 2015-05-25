﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Free_Sharp_Player {
	class ExtraMenuModel : ViewModelNotifier {
		public String Votes { get { return GetProp<String>(); } set { SetProp(value); } }

		//TODO: set votes to "---" when stopped.


		private MainWindow window;
		private StreamManager streamManager;

		public ExtraMenuModel(MainWindow win, StreamManager man) {
			window = win;
			streamManager = man;

			streamManager.OnEventTrigger += OnEvent;
			streamManager.OnQueueTick += OnTick;
			streamManager.OnBufferingStateChange += OnBufferChange;

			window.Extras.DataContext = this;

			window.btn_Like.Click += ResetCapture;
			window.btn_Dislike.Click += ResetCapture;
			window.btn_Favor.Click += ResetCapture;
			window.btn_Request.Click += ResetCapture;
			window.btn_Settings.Click += ResetCapture;

			Votes = "---";
		}

		public void OnEvent(EventTuple ev) { }

		public void OnTick(QueueSettingsTuple set) { }

		public void OnBufferChange(bool isBuffering) { }

		public void UpdateInfo(getRadioInfo info) {
			window.Dispatcher.Invoke(new Action(() => {
				if (info != null && !String.IsNullOrEmpty(info.rating))
					Votes = info.rating;
			}));
		}

		private void ResetCapture(object o, object e) {
			Mouse.Capture(window.Extras, CaptureMode.SubTree);
		}
	}
}

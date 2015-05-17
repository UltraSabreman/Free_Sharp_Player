using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Free_Sharp_Player {
	class VolumeModel : ViewModelNotifier {

		private double oldVolume = 0;
		public double Volume { get { return GetProp<double>(); } set { SetProp(value); } }
		public bool Mute { get { return GetProp<bool>(); } set { 
			if (value) {
				oldVolume = Volume;
				Volume = 0;
			} else
				Volume = oldVolume;
			SetProp(value); 
		} }

		private MainWindow window;

		public VolumeModel(MainWindow win) {
			window = win;

			window.Volume.DataContext = this;

			Mute = false;

			window.sldr_VolumeSlider.ValueChanged += sldr_VolumeSlider_ValueChanged;
			window.btn_MuteButton.Click += btn_MuteButton_Click;

			window.sldr_VolumeSlider.MouseLeave += ResetCapture;
			window.btn_MuteButton.Click += ResetCapture;

			Volume = 25; //TODO: save/load this
		}

		public void Tick(Object o, EventArgs e) {
			new Thread(() => { }).Start();
		}

		private void ResetCapture(object o, object e) {
			Mouse.Capture(window.Volume, CaptureMode.SubTree);
		}

		private void sldr_VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			window.SetVolume(window.sldr_VolumeSlider.Value);

		}

		private void btn_MuteButton_Click(object sender, RoutedEventArgs e) {
			Mute = !Mute;
			window.sldr_VolumeSlider.IsEnabled = !Mute;
		}
	}
}

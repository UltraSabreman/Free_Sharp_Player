using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Free_Sharp_Player {
	public class MainModel : ViewModelNotifier {
		public String StreamTitle { get { return GetProp<String>(); } set { SetProp(value); } }
		public int PosInBuffer { get { return GetProp<int>(); } set { SetProp(value); } }
		public int BufferLen { get { return GetProp<int>(); } set { SetProp(value); } }

		private MainWindow window;

		private bool VolumeOpen = false;
		private bool ExtrasOpen = false;
		MouseButtonEventHandler VolumeOutClick;
		MouseButtonEventHandler ExtrasOutClick;

		//double click on thing to open playlist

		public MainModel(MainWindow win) {
			window = win;

			window.bar_Buffer.DataContext = this;
			window.bar_BufferWindow.DataContext = this;
			window.lbl_TrackName.DataContext = this;

			window.btn_PlayPause.Click += btn_PlayPause_Click;
			window.btn_Volume.Click += btn_Volume_Click;
			window.btn_Extra.Click += btn_Extra_Click;

			VolumeOutClick = new MouseButtonEventHandler(HandleClickOutsideOfVolume);
			ExtrasOutClick = new MouseButtonEventHandler(HandleClickOutsideOfExtras);

			window.theStreamer.OnBufferChange += (i) => {
				BufferLen = i;
			};
		}

		private void btn_PlayPause_Click(object sender, RoutedEventArgs e) {
			if (window.GetStreamState() == MP3Stream.StreamingPlaybackState.Playing) {
				window.Pause();
				window.btn_PlayPause.Content = "▶";
			} else {
				window.Play();
				window.btn_PlayPause.Content = "▌▐";
			}
		}

		private void HandleClickOutsideOfVolume(object sender, MouseButtonEventArgs e) {
			window.VolumeSlider.ReleaseMouseCapture();
			window.VolumeSlider.RemoveHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, VolumeOutClick);

			Console.WriteLine("OutVolClick");

			if (!window.btn_Volume.IsMouseOver && VolumeOpen)
				btn_Volume_Click(null, null);
		}

		private void HandleClickOutsideOfExtras(object sender, MouseButtonEventArgs e) {
			window.ExtrasMenu.ReleaseMouseCapture();
			window.ExtrasMenu.RemoveHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, ExtrasOutClick);
			Console.WriteLine("OutExtClick");

			if (!window.btn_Extra.IsMouseOver && ExtrasOpen)
				btn_Extra_Click(null, null);
		}

		private void btn_Volume_Click(object sender, RoutedEventArgs e) {
			VolumeOpen = !VolumeOpen;
			ThicknessAnimation testan;
			if (VolumeOpen) {
				Mouse.Capture(window.VolumeSlider, CaptureMode.SubTree);
				window.VolumeSlider.AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, VolumeOutClick, true);
				
				testan = new ThicknessAnimation(new Thickness(0, 120, 0, 0), new Thickness(0, 0, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
			} else {
				testan = new ThicknessAnimation(new Thickness(0, 0, 0, 0), new Thickness(0, 120, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
			}

			Storyboard test = new Storyboard();
			test.Children.Add(testan);
			Storyboard.SetTargetName(testan, window.VolumeSlider.Name);
			Storyboard.SetTargetProperty(testan, new PropertyPath(Grid.MarginProperty));
			test.Begin(window.VolumeSlider);
		}



		private void btn_Extra_Click(object sender, RoutedEventArgs e) {
			ExtrasOpen = !ExtrasOpen;
			ThicknessAnimation testan;
			if (ExtrasOpen) {
				Mouse.Capture(window.ExtrasMenu, CaptureMode.SubTree);
				window.ExtrasMenu.AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, ExtrasOutClick, true);

				testan = new ThicknessAnimation(new Thickness(0, 60, 0, 0), new Thickness(0, 0, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
			} else {
				testan = new ThicknessAnimation(new Thickness(0, 0, 0, 0), new Thickness(0, 60, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
			}

			Storyboard test = new Storyboard();
			test.Children.Add(testan);
			Storyboard.SetTargetName(testan, window.ExtrasMenu.Name);
			Storyboard.SetTargetProperty(testan, new PropertyPath(Grid.MarginProperty));
			test.Begin(window.ExtrasMenu);
		}

	}
}

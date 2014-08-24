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
		public String StreamTitle { get { return GetProp<String>(); } set { SetProp(value); DoMarquee(); } }
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
			window.txt_TrackName.DataContext = this;

			window.btn_PlayPause.Click += btn_PlayPause_Click;
			window.btn_Volume.Click += btn_Volume_Click;
			window.btn_Extra.Click += btn_Extra_Click;

			VolumeOutClick = new MouseButtonEventHandler(HandleClickOutsideOfVolume);
			ExtrasOutClick = new MouseButtonEventHandler(HandleClickOutsideOfExtras);

			window.UpdateLayout();
		}

		private void DoMarquee() {
			DoubleAnimation doubleAnimation = new DoubleAnimation();
			Canvas canvas = window.can_TrackName;
			TextBlock text = window.txt_TrackName;
			TextBlock text2 = window.txt_TrackName2;

			if (canvas != null && text != null && text.ActualWidth >= canvas.ActualWidth) {
				text2.Visibility = Visibility.Visible;
				//todo better marquee math here.
				DoubleAnimation anim = new DoubleAnimation(0, (text.ActualWidth + canvas.ActualWidth / 2), new Duration(new TimeSpan(0, 0, 10)));
				DoubleAnimation anim2 = new DoubleAnimation(-(text.ActualWidth + canvas.ActualWidth / 2), 0, new Duration(new TimeSpan(0, 0, 10)));
				anim.RepeatBehavior = RepeatBehavior.Forever;
				anim2.RepeatBehavior = RepeatBehavior.Forever;

				text.BeginAnimation(Canvas.RightProperty, anim);
				text2.BeginAnimation(Canvas.RightProperty, anim2);
			} else
				text2.Visibility = Visibility.Hidden;

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
			Console.WriteLine("OutVolClick");
			window.Volume.ReleaseMouseCapture();
			window.Volume.RemoveHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, VolumeOutClick);


			if (!window.btn_Volume.IsMouseOver && VolumeOpen)
				btn_Volume_Click(null, null);
		}

		private void HandleClickOutsideOfExtras(object sender, MouseButtonEventArgs e) {
			Console.WriteLine("OutExtClick");
			window.Extras.ReleaseMouseCapture();
			window.Extras.RemoveHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, ExtrasOutClick);

			if (!window.btn_Extra.IsMouseOver && ExtrasOpen)
				btn_Extra_Click(null, null);
		}

		private void btn_Volume_Click(object sender, RoutedEventArgs e) {
			VolumeOpen = !VolumeOpen;
			ThicknessAnimation testan;
			if (VolumeOpen) {
				Mouse.Capture(window.Volume, CaptureMode.SubTree);
				window.Volume.AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, VolumeOutClick, true);
				
				testan = new ThicknessAnimation(new Thickness(0, 120, 0, 0), new Thickness(0, 0, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
			} else {
				testan = new ThicknessAnimation(new Thickness(0, 0, 0, 0), new Thickness(0, 120, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
			}

			Storyboard test = new Storyboard();
			test.Children.Add(testan);
			Storyboard.SetTargetName(testan, window.Volume.Name);
			Storyboard.SetTargetProperty(testan, new PropertyPath(Grid.MarginProperty));
			test.Begin(window.Volume);
		}



		private void btn_Extra_Click(object sender, RoutedEventArgs e) {
			ExtrasOpen = !ExtrasOpen;
			ThicknessAnimation testan;
			if (ExtrasOpen) {
				Mouse.Capture(window.Extras, CaptureMode.SubTree);
				window.Extras.AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, ExtrasOutClick, true);

				testan = new ThicknessAnimation(new Thickness(0, 60, 0, 0), new Thickness(0, 0, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
			} else {
				testan = new ThicknessAnimation(new Thickness(0, 0, 0, 0), new Thickness(0, 60, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
			}

			Storyboard test = new Storyboard();
			test.Children.Add(testan);
			Storyboard.SetTargetName(testan, window.Extras.Name);
			Storyboard.SetTargetProperty(testan, new PropertyPath(Grid.MarginProperty));
			test.Begin(window.Extras);
		}



	}
}

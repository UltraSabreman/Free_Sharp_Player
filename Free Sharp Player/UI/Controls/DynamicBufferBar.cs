using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.ComponentModel;

namespace Free_Sharp_Player {
	public class DynamicBufferBar : Control {
		static DynamicBufferBar() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DynamicBufferBar), new FrameworkPropertyMetadata(typeof(DynamicBufferBar)));
		}

		#region depProperties
		/// <summary>
		/// The total amount of music (in seconds) that can be 
		/// loaded into memory
		/// </summary>
		public double MaxBufferSize {
			get { return (double) GetValue(MaxBufferSizeProperty); }
			set { SetValue(MaxBufferSizeProperty, value);  }
		}
		/// <summary>
		/// The total amount of music (in seconds) currentlly in memory
		/// </summary>
		public double TotalBufferSize {
			get { return (double)GetValue(TotalBufferSizeProperty); }
			set { SetValue(TotalBufferSizeProperty, value); }
		}
		/// <summary>
		// Current stream title
		/// </summary>
		public String StreamTitle {
			get { return (String)GetValue(StreamTitleSizeProperty); }
			set { SetValue(StreamTitleSizeProperty, value); }
		}
		/// <summary>
		/// The total amount of music (in seconds) currentlly in memory
		/// </summary>
		public double SongMaxLength {
			get { return (double)GetValue(SongMaxLengthProperty); }
			set { SetValue(SongMaxLengthProperty, value); }
		}
		/// <summary>
		// Current stream title
		/// </summary>
		public double SongLength {
			get { return (double)GetValue(SongLengthProperty); }
			set { SetValue(SongLengthProperty, value);}
		}
		/// <summary>
		/// The total amount of music (in seconds) that's already been
		/// played
		/// </summary>
		public double PlayedBufferSize {
			get { return (double)GetValue(PlayedBufferSizeProperty); }
			set { 
				SetValue(PlayedBufferSizeProperty, value);

				if (theCanvas == null) return;
				double hpos = theCanvas.ActualWidth - (((PlayedBufferSize / MaxBufferSize) * theCanvas.ActualWidth) + 5);
				HandlePosition = hpos;
			}
		}

		public double HandlePosition {
			get { return (double)GetValue(HandlePositionProperty); }
			private set { SetValue(HandlePositionProperty, value); }
		}

		public static readonly DependencyProperty StreamTitleSizeProperty	= DependencyProperty.Register("StreamTitle", typeof(String), typeof(DynamicBufferBar), null);
		public static readonly DependencyProperty MaxBufferSizeProperty = DependencyProperty.Register("MaxBufferSize", typeof(double), typeof(DynamicBufferBar), null);
		public static readonly DependencyProperty TotalBufferSizeProperty = DependencyProperty.Register("TotalBufferSize", typeof(double), typeof(DynamicBufferBar), new PropertyMetadata(Buff));
		public static readonly DependencyProperty PlayedBufferSizeProperty	= DependencyProperty.Register("PlayedBufferSize", typeof(double), typeof(DynamicBufferBar), null);
		public static readonly DependencyProperty HandlePositionProperty	= DependencyProperty.Register("HandlePosition", typeof(double), typeof(DynamicBufferBar), null);
		public static readonly DependencyProperty SongMaxLengthProperty = DependencyProperty.Register("SongMaxLength", typeof(double), typeof(DynamicBufferBar), new PropertyMetadata(SongLenChanged));
		public static readonly DependencyProperty SongLengthProperty = DependencyProperty.Register("SongLength", typeof(double), typeof(DynamicBufferBar), new PropertyMetadata(SongLenChanged));
		#endregion

		private static void SongLenChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
		   DynamicBufferBar h = sender as DynamicBufferBar;
		   if (h != null)  {
			  h.RedoToolTip(); 
		   }
		}

		private static void Buff(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
			DynamicBufferBar h = sender as DynamicBufferBar;
			if (h != null) {
				Util.Print("====CHANGED====");
			}
		}

		public delegate void SeekDone(double secDiffrence);
		public event SeekDone OnSeekDone;

		private ProgressBar totalBuffer;
		private ProgressBar playedBuffer;
		private Canvas theCanvas;
		private Button bufferHandle;

		private double oldtime = 0;
		private bool isHeld = false;
		private BitmapImage songChangeImage;
		private BitmapImage disconnectImage;
		private List<Image> markers = new List<Image>();

		protected override void OnInitialized(EventArgs e) {
			base.OnInitialized(e);

			songChangeImage = new BitmapImage();
			songChangeImage.BeginInit();
			songChangeImage.UriSource = new Uri(System.IO.Path.Combine(Directory.GetCurrentDirectory(), @"Resources\songchange.png"));
			songChangeImage.EndInit();

			disconnectImage = new BitmapImage();
			disconnectImage.BeginInit();
			disconnectImage.UriSource = new Uri(System.IO.Path.Combine(Directory.GetCurrentDirectory(), @"Resources\disconnect.png"));
			disconnectImage.EndInit();
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
			totalBuffer = GetTemplateChild("BufferedProgress") as ProgressBar;
			playedBuffer = GetTemplateChild("PlayedProgress") as ProgressBar;
			bufferHandle = GetTemplateChild("BufferHandle") as Button;
			theCanvas = GetTemplateChild("Surface") as Canvas;

			bufferHandle.PreviewMouseDown += bufferHandle_MouseDown;
			bufferHandle.PreviewMouseUp += bufferHandle_MouseUp;
			bufferHandle.PreviewMouseMove += bufferHandle_MouseMove;

			theCanvas.UpdateLayout();
		}

		
		public void RedoToolTip() {
			if (SongLength < 0)
				ToolTip = "--:-- / --:--";
			else
				ToolTip = String.Format("{0:D}:{1:D2} / {2:D}:{3:D2}", (int)SongLength/60, (int) SongLength%60, (int)SongMaxLength/60, (int)SongMaxLength%60);
		}

		void bufferHandle_MouseUp(object sender, MouseButtonEventArgs e) {
			isHeld = false;

			if (OnSeekDone != null)
				OnSeekDone(oldtime - PlayedBufferSize);
		}

		void bufferHandle_MouseMove(object sender, MouseEventArgs e) {
			if (!isHeld) return;

			var pos = e.MouseDevice.GetPosition(theCanvas);
			PlayedBufferSize = Math.Min((pos.X / theCanvas.ActualWidth) * MaxBufferSize, TotalBufferSize);
			PlayedBufferSize = Math.Max(PlayedBufferSize, 0);

			Util.PrintLine(PlayedBufferSize);

		}

		void bufferHandle_MouseDown(object sender, MouseButtonEventArgs e) {
			isHeld = true;
			oldtime = PlayedBufferSize;

			//TODO: fire PauseEvent
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
 			 base.OnRenderSizeChanged(sizeInfo);

			 PlayedBufferSize++;
			 PlayedBufferSize--;
		}

		public void Update(List<EventTuple> events) {
			int i = 0;
			foreach (EventTuple e in events) {
				if (e.Event == EventType.None) continue;

				Image image;
				if (i < markers.Count)
					image = markers[i];
				else {
					image = new Image();
					image.IsHitTestVisible = false;
					markers.Add(image);
					theCanvas.Children.Add(image);
				}
				i++;

				BitmapImage thePic;
				if (e.Event == EventType.Disconnect)
					thePic = disconnectImage;
				else
					thePic = songChangeImage;

				image.Source = thePic;
				image.SetValue(Canvas.TopProperty, theCanvas.ActualHeight / 2 - thePic.Height / 2);
				double pos = theCanvas.ActualWidth - ((e.EventQueuePosition / TotalBufferSize) * theCanvas.ActualWidth);
				image.SetValue(Canvas.RightProperty, pos);
			}

		}


	}
}

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
using System.Threading;
using System.Windows.Threading;

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
			set {
				if (!isHeld) {
					SetValue(TotalBufferSizeProperty, value);
					if (TotalBufferSize > MaxBufferSize)
						MaxBufferSize += 1;
				}
			}
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
				//Playedchanged();
			}
		}

		public double HandlePosition {
			get { return (double)GetValue(HandlePositionProperty); }
			private set { SetValue(HandlePositionProperty, value); }
		}

		public static readonly DependencyProperty StreamTitleSizeProperty	= DependencyProperty.Register("StreamTitle", typeof(String), typeof(DynamicBufferBar), null);
		public static readonly DependencyProperty MaxBufferSizeProperty = DependencyProperty.Register("MaxBufferSize", typeof(double), typeof(DynamicBufferBar), null);
		public static readonly DependencyProperty TotalBufferSizeProperty = DependencyProperty.Register("TotalBufferSize", typeof(double), typeof(DynamicBufferBar), null);
		public static readonly DependencyProperty PlayedBufferSizeProperty = DependencyProperty.Register("PlayedBufferSize", typeof(double), typeof(DynamicBufferBar), new PropertyMetadata(Buff));
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
				h.Playedchanged();
			}
		}

		public delegate void SeekDone(double secDiffrence);
		public event SeekDone OnSeekDone;

		private ProgressBar totalBuffer;
		private ProgressBar playedBuffer;
		private Canvas theCanvas;
		private Button bufferHandle;
		private MarqueeTextBlock mark;

		private double oldtime = 0;
		private bool isHeld = false;
		private BitmapImage songChangeImage;
		private BitmapImage disconnectImage;
		private List<Image> markers = new List<Image>();

		public DynamicBufferBar()
			: base() {
			TotalBufferSize = 0;
			MaxBufferSize = 0;
			PlayedBufferSize = 0;
			SongLength = -1;
			SongMaxLength = 0;
			StreamTitle = "";
			HandlePosition = 0;

			songChangeImage = new BitmapImage();
			songChangeImage.BeginInit();
			songChangeImage.UriSource = new Uri("/Free Sharp Player;component/Resources/songchange.png", UriKind.RelativeOrAbsolute);// new Uri(System.IO.Path.Combine(Directory.GetCurrentDirectory(), @"Resources\songchange.png"));
			songChangeImage.EndInit();

			disconnectImage = new BitmapImage();
			disconnectImage.BeginInit();
			disconnectImage.UriSource = new Uri("/Free Sharp Player;component/Resources/disconnect.png", UriKind.RelativeOrAbsolute);// new Uri(System.IO.Path.Combine(Directory.GetCurrentDirectory(), @"Resources\disconnect.png"));
			disconnectImage.EndInit();

			Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() => { done = true; }));
		}

		private bool done = false;

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
			totalBuffer = GetTemplateChild("BufferedProgress") as ProgressBar;
			playedBuffer = GetTemplateChild("PlayedProgress") as ProgressBar;
			bufferHandle = GetTemplateChild("BufferHandle") as Button;
			theCanvas = GetTemplateChild("Surface") as Canvas;
			mark = GetTemplateChild("StreamTitle") as MarqueeTextBlock;

			bufferHandle.PreviewMouseDown += bufferHandle_MouseDown;
			bufferHandle.PreviewMouseUp += bufferHandle_MouseUp;
			bufferHandle.PreviewMouseMove += bufferHandle_MouseMove;

			theCanvas.UpdateLayout();
			Playedchanged();
			RedoToolTip();
		}

		
		public void RedoToolTip() {
			if (double.IsNaN(SongLength) || double.IsInfinity(SongLength) || SongLength < 0)
				ToolTip = "--:-- / --:--";
			else
				ToolTip = String.Format("{0:D}:{1:D2} / {2:D}:{3:D2}", (int)SongLength/60, (int) SongLength%60, (int)SongMaxLength/60, (int)SongMaxLength%60);
		}


		public void Playedchanged() {
			if (theCanvas == null) return;

			double hpos = theCanvas.ActualWidth - (((PlayedBufferSize / MaxBufferSize) * theCanvas.ActualWidth) + 5);
			if (!double.IsNaN(hpos) && !double.IsInfinity(hpos))
				HandlePosition = hpos;
		}


		void bufferHandle_MouseUp(object sender, MouseButtonEventArgs e) {
			isHeld = false;

			if (OnSeekDone != null)
				OnSeekDone(0-(oldtime - PlayedBufferSize));
		}

		void bufferHandle_MouseMove(object sender, MouseEventArgs e) {
			if (!isHeld) return;

			var pos = e.MouseDevice.GetPosition(theCanvas);
			PlayedBufferSize = Math.Min((pos.X / theCanvas.ActualWidth) * MaxBufferSize, TotalBufferSize);
			PlayedBufferSize = Math.Max(PlayedBufferSize, 0);

			Playedchanged();
		}

		void bufferHandle_MouseDown(object sender, MouseButtonEventArgs e) {
			isHeld = true;
			oldtime = PlayedBufferSize;
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
 			 base.OnRenderSizeChanged(sizeInfo);

			 PlayedBufferSize++;
			 PlayedBufferSize--;
		}

		public void Update(List<MusicStream.EventTuple> events) {
			mark.DoMarqueeLogic();

			if (theCanvas == null) return;
			if (events == null && theCanvas != null) {
				
				var slider = theCanvas.Children[0];
				theCanvas.Children.Clear();
				theCanvas.Children.Add(slider);

				PlayedBufferSize = 0;
				TotalBufferSize = 0;
				return;
			}

			if (!done) return;
			int i = 0;
            lock (MusicStream.addLock) {
                foreach (MusicStream.EventTuple e in events) {
                    if (e.Event == EventType.None || e.EventQueuePosition == 0) continue;

                    Image image;
                    if (i < markers.Count) {
                        image = markers[i];
                        image.Visibility = System.Windows.Visibility.Visible;
                    } else {
                        image = new Image();
                        image.IsHitTestVisible = false;
                        markers.Add(image);
                        theCanvas.Children.Add(image);
                    }
                    i++;

                    BitmapImage thePic;
                    if (e.Event == EventType.StateChange)
                        thePic = disconnectImage;
                    else
                        thePic = songChangeImage;

                    image.Source = thePic;
                    image.Height = theCanvas.ActualHeight;
                    image.SetValue(Canvas.TopProperty, theCanvas.ActualHeight / 2 - thePic.PixelHeight / 2);


                    if (theCanvas == null || double.IsNaN(theCanvas.ActualWidth) || double.IsInfinity(theCanvas.ActualWidth))
                        continue;

                    double lol = theCanvas.ActualWidth;

                    if (TotalBufferSize != 0 && MaxBufferSize != 0) {
                        double sizeOfProgress = (TotalBufferSize / MaxBufferSize) * theCanvas.ActualWidth;
                        double pos = lol - (((e.EventQueuePosition / TotalBufferSize) * sizeOfProgress) + (thePic.PixelWidth / 2));
                        if (!double.IsNaN(pos) && !double.IsInfinity(pos))
                            image.SetValue(Canvas.RightProperty, pos);

                    }

                }
            }

			for (; i < markers.Count; i++) {
				markers[i].Visibility = System.Windows.Visibility.Collapsed;
			}
		}


	}
}

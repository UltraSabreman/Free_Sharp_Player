using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Free_Sharp_Player.UI {
	/// <summary>
	/// Interaction logic for TestWindow.xaml
	/// </summary>
	public partial class TestWindow : Window {
		[DllImport("kernel32")]
		static extern bool AllocConsole();

		public TestWindow() {
			InitializeComponent();
			AllocConsole();
			/*MyBuffer.MaxBufferSize = 100;
			MyBuffer.TotalBufferSize = 75;
			MyBuffer.PlayedBufferSize = MyBuffer.TotalBufferSize - 15;*/
			prog.Value = 0;

			Timer lol = new Timer();
			lol.Interval = 1000;
			lol.AutoReset = false;
			lol.Enabled = true;
			lol.Elapsed += (o, e) => {
				Dispatcher.Invoke(new Action(() => {
					MyTextControlTest.Content = "~~~~Dat Scrolling Text~~~~";

					List<MusicStream.EventTuple> events = new List<MusicStream.EventTuple>() {
						new MusicStream.EventTuple(EventType.SongChange, StreamState.None) {EventQueuePosition = 25},
						new MusicStream.EventTuple(EventType.StateChange, StreamState.None) {EventQueuePosition = 50}
					};

					//MyBuffer.Update("Some long stream name - artist", 100, 90, 75, 120, 35, events);
				}));
			};
			lol.Start();
		}
	}
}

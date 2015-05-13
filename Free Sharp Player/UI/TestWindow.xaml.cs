using System;
using System.Collections.Generic;
using System.Linq;
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
		public TestWindow() {
			InitializeComponent();

			bool asdf = true;
			MyBuffer.MaxBufferSize = 100;
			MyBuffer.TotalBufferSize = 0;
			prog.Value = 0;

			Timer lol = new Timer();
			lol.Interval = 25;
			lol.AutoReset = true;
			lol.Enabled = true;
			lol.Elapsed += (o, e) => {
				Dispatcher.Invoke(new Action(() => {
					MyTextControlTest.Content = "~~~~Dat Scrolling Text~~~~";

					if (asdf) {
						MyBuffer.TotalBufferSize++;
						prog.Value++;
					} else {
						MyBuffer.TotalBufferSize--;
						prog.Value--;
					}

					if (prog.Value == 100 || prog.Value == 0)
						asdf = !asdf;
				}));
			};
			lol.Start();
		}
	}
}

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

			Timer lol = new Timer();
			lol.Interval = 1000;
			lol.AutoReset = true;
			lol.Enabled = true;
			lol.Elapsed += (o, e) => {
				MyTextControlTest.Content = "~~~~Dat Scrolling Text~~~~";
			};
		}
	}
}

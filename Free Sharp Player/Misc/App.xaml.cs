using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Free_Sharp_Player {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		ConsoleRunner theThing = new ConsoleRunner();
		public App() {
			theThing.Run();
		}
	}
}

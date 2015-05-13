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
		public double MaxBufferSize { get; set; }
		/// <summary>
		/// The total amount of music (in seconds) currentlly in memory
		/// </summary>
		public double TotalBufferSize { get; set; }
		/// <summary>
		/// The total amount of music (in seconds) that's already been
		/// played
		/// </summary>
		public double PlayedBufferSize { get; set; }

		public static readonly DependencyProperty MaxBufferSizeProperty = DependencyProperty.Register("MaxBufferSize", typeof(double), typeof(DynamicBufferBar), null);
		public static readonly DependencyProperty TotalBufferSizeProperty = DependencyProperty.Register("TotalBufferSize", typeof(double), typeof(DynamicBufferBar), null);
		public static readonly DependencyProperty PlayedBufferSizeProperty = DependencyProperty.Register("PlayedBufferSize", typeof(double), typeof(DynamicBufferBar), null);
		#endregion

		

		public DynamicBufferBar() : base() {
			MaxBufferSize = 100;
			TotalBufferSize = 50;
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
			/*var PART_Track = GetTemplateChild("PART_Track") as Image;

			BitmapImage logo = new BitmapImage();
			logo.BeginInit();
			logo.UriSource = new Uri(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Resources\\stripes.png"));
			logo.EndInit();

			PART_Track.Source = logo;*/
		}




	}
}

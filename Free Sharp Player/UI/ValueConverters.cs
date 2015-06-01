using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Free_Sharp_Player {
	public class SliderToVolumeConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			String s = ((int)(double)value).ToString();
			return s;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			// Do the conversion from visibility to bool
			throw new NotImplementedException();
		}
	}
	public class BoolToMuteIcon : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			bool val = (bool)value;
			return val ? "UM" : "M";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			// Do the conversion from visibility to bool
			throw new NotImplementedException();
		}
	}

	public class ReqToBoolConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (((int) value) == 0)
				return false;
			else
				return true;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			// Do the conversion from visibility to bool
			throw new NotImplementedException();
		}
	}

	public class SongLengthSafetyConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			double len = (double)value;
			if (len < 0)
				return 1;
			else
				return len;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			// Do the conversion from visibility to bool
			throw new NotImplementedException();
		}
	}


	public class SongLengthToTimeConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			double len = (double)value;
			if (len < 0)
				return "Brodcast is Live";
			else
				return String.Format("{0:D}:{1:D2}", (int)len/60, (int) len%60);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			// Do the conversion from visibility to bool
			throw new NotImplementedException();
		}
	}

	public class SongLengthToColorConverter : IValueConverter {
		private static BrushConverter con = new BrushConverter();
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			double len = (double)value;
			return (len > 0 ? con.ConvertFromString("#B27C30") : con.ConvertFromString("#D91E18")) as Brush;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			// Do the conversion from visibility to bool
			throw new NotImplementedException();
		}
	}

	public class LastPlayedConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			String date = (String)value;
			DateTime lastPlayedDate = DateTime.Parse(date);//TimeZoneInfo.ConvertTime(DateTime.Parse(date), hwZone, TimeZoneInfo.Local);
			return lastPlayedDate.ToString("h:mm");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			// Do the conversion from visibility to bool
			throw new NotImplementedException();
		}
	}

	public class TimeInQueueConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			double ticks;
			if (value.GetType() == typeof(double))
				ticks = (double)value; 
			else 
				ticks = (int)value;


			System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds(ticks).ToLocalTime();
			TimeSpan timeInQueue = DateTime.Now - dtDateTime;
			return String.Format("{0}:{1}", timeInQueue.Minutes.ToString(), timeInQueue.Seconds.ToString("D2"));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			// Do the conversion from visibility to bool
			throw new NotImplementedException();
		}
	}
}

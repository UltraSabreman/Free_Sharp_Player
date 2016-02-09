using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows;
using System.Windows.Media.Animation;

namespace Free_Sharp_Player {

	using DataDict = Dictionary<String, Object>;
	static class Util {
		public static void Print() { Print(null); }
		/// <summary>
		/// Lets you print stuff. Any number of objects.
		/// To change forground color, pass one console color and then another object (ie: print(ConsoleColor.Red, "this will be red")).
		/// To change both fore and back colors, path two console colors then another object (ie: print(ConsoleColor.Red, ConsoleColor.White, "this will be red on white")).
		/// </summary>
		/// <param name="stuff">Console colors, and objects to print.</param>
		public static void PrintLine() { PrintLine(null); }
		public static void PrintLine(params object[] stuff) { Print(stuff); Console.WriteLine(); }
		public static void Print(params object[] stuff) {
			if (stuff == null) {
				Console.WriteLine();
				return;
			}

			ConsoleColor oldf = ConsoleColor.Gray;// Console.ForegroundColor;
			ConsoleColor oldb = ConsoleColor.Black;// Console.BackgroundColor;

			var enumerator = stuff.GetEnumerator();

			while (enumerator.MoveNext()) {
				Object o = enumerator.Current;

				if (o is ConsoleColor) {
					Console.ForegroundColor = ((ConsoleColor)o);
					enumerator.MoveNext();
					if (enumerator.Current is ConsoleColor) {
						Console.BackgroundColor = ((ConsoleColor)enumerator.Current);
						enumerator.MoveNext();
					}
					Console.Write(enumerator.Current.ToString());
				} else
					Console.Write(enumerator.Current.ToString());

				Console.ForegroundColor = oldf;
				Console.BackgroundColor = oldb;
			}
		}

		public static String trimDateString(String date) {
			return date.Split(":".ToCharArray(), 2)[1];
		}


		public static void AnimateWindowMovex(Window window, double width) {
			DoubleAnimation moveXAnimation = new DoubleAnimation(window.Left, window.Left + width, new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);

			Storyboard moveXStoryboard = new Storyboard();
			moveXStoryboard.Children.Add(moveXAnimation);
			Storyboard.SetTargetName(moveXAnimation, window.Name);
			Storyboard.SetTargetProperty(moveXAnimation, new PropertyPath(Window.LeftProperty));
			moveXStoryboard.Begin(window);
		}

		public static void AnimateWindowMoveY(Window window, double height) {
			DoubleAnimation moveYAnimation = new DoubleAnimation(window.Top, window.Top + height, new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
			
			Storyboard moveYStoryboard = new Storyboard();
			moveYStoryboard.Children.Add(moveYAnimation);
			Storyboard.SetTargetName(moveYAnimation, window.Name);
			Storyboard.SetTargetProperty(moveYAnimation, new PropertyPath(Window.TopProperty));
			moveYStoryboard.Begin(window);
		}

		public static void DumpException(Exception e) {
			Func<int, String> getEquals = (count) => {
				String s = "";
				for (int i = 0; i < count; i++)
					s += "=";

				return s;
			};

			PrintLine(ConsoleColor.White, ConsoleColor.Red, "====EXCEPTION====");
			PrintLine(ConsoleColor.White, "=MSG: ", ConsoleColor.Red, e.Message);
			PrintLine(ConsoleColor.White, "=SRC: ", ConsoleColor.Red, e.Source);
			PrintLine(ConsoleColor.White, "=TGT: ", ConsoleColor.Red, e.TargetSite);
			PrintLine(ConsoleColor.White, "=ST : ", ConsoleColor.Red, e.StackTrace);
			e = e.InnerException;
			int ind = 1;
			while (e != null) {
				String eq = getEquals(ind++);
				PrintLine(ConsoleColor.White, ConsoleColor.Red, eq + "===EXCEPTION====");
				PrintLine(ConsoleColor.White, eq + "MSG: ", ConsoleColor.Red, e.Message);
				PrintLine(ConsoleColor.White, eq + "SRC: ", ConsoleColor.Red, e.Source);
				PrintLine(ConsoleColor.White, eq + "TGT: ", ConsoleColor.Red, e.TargetSite);
				PrintLine(ConsoleColor.White, eq + "ST : ", ConsoleColor.Red, e.StackTrace);
				e = e.InnerException;
			}

			PrintLine(ConsoleColor.White, ConsoleColor.Red, "=================");
		}

		public static Object DictKeyChain(DataDict dict, params String[] keys) {
			Object curDic = dict;

			foreach (String key in keys) {
				if (curDic.GetType() == typeof(DataDict)) {
					curDic = ((DataDict)curDic)[key];
				} else
					return curDic;
			}

			return curDic;
		}


		public static Dictionary<String, String> StringToDict(String msg) {
			if (msg == null) return null;

			var outd = new Dictionary<String, String>();

			var test = JsonConvert.DeserializeObject(msg) as JObject;
			foreach (JProperty p in test.Properties())
				outd[p.Name] = p.Value.ToString();


			return outd;
		}

		public static bool ToggleAllowUnsafeHeaderParsing(bool enable) {
			//Get the assembly that contains the internal class
			Assembly assembly = Assembly.GetAssembly(typeof(SettingsSection));
			if (assembly != null) {
				//Use the assembly in order to get the internal type for the internal class
				Type settingsSectionType = assembly.GetType("System.Net.Configuration.SettingsSectionInternal");
				if (settingsSectionType != null) {
					//Use the internal static property to get an instance of the internal settings class.
					//If the static instance isn't created already invoking the property will create it for us.
					object anInstance = settingsSectionType.InvokeMember("Section",
					BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });
					if (anInstance != null) {
						//Locate the private bool field that tells the framework if unsafe header parsing is allowed
						FieldInfo aUseUnsafeHeaderParsing = settingsSectionType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
						if (aUseUnsafeHeaderParsing != null) {
							aUseUnsafeHeaderParsing.SetValue(anInstance, enable);
							return true;
						}

					}
				}
			}
			return false;
		}
	}
}

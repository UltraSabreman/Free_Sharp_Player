using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
		public static void Print(params object[] stuff) {
			if (stuff == null) {
				Console.WriteLine();
				return;
			}

			ConsoleColor oldf = Console.ForegroundColor;
			ConsoleColor oldb = Console.BackgroundColor;

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

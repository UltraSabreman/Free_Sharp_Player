using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using Newtonsoft.Json;

namespace Free_Sharp_Player {
	using ConfigMap = Dictionary<String, Object>;
	using System.Text.RegularExpressions;

	public static class Configs {
		//This is a super convaluted way to make sure we always put the config file in the same dir as this assembly.
		private static String path = new Uri(new Regex(@"(/[^/]*$)").Replace(System.Reflection.Assembly.GetExecutingAssembly().CodeBase, "/configs.json")).LocalPath;
		/// <summary>
		/// whether the configs are saved automaticly each time they are changed.
		/// </summary>
		public static bool AutoSave = false;


		//Here you can define defaults for all values. This is a string-object dictionary so anything can be used for a value.
		private static ConfigMap Settings = new ConfigMap() {
			{ "AppID", "ultrasaberman.freesharpplayer" },
			{ "MaxBufferLenSec", 30},
			{ "MinBufferLenSec", 3},
            { "MaxTotalBufferdSongSec", 300},

            { "Volume", 0.5 }
		};

		/// <summary>
		/// Initilizes the configs by trying to read from the settings file.
		/// If something goes wrong, defaults are loaded and the file is overwritten.
		/// </summary>
		static Configs() {
			try {
				ReadConfigs();
			} catch (Exception) {
				//Trys to write the default settings.
				try {
					WriteConfigs();
				} catch (Exception) { }
			} finally {
				Initilize();
			}
		}

        public static T Get<T>(String key) {
            if (Settings.ContainsKey(key))
                return (T)Settings[key];
            else
                return default(T);
        }

        public static Object Get(String key) {
            if (Settings.ContainsKey(key))
                return Settings[key];
            else
                return null;
        }

        public static void Set(String key, Object value) {
            Settings[key] = value;
            if (AutoSave)
                WriteConfigs();
        }

		//Reads the config file
		public static void ReadConfigs() {
			using (StreamReader rd = new StreamReader(path)) {
				//Replace with your preffered method of de-serialization
				ConfigMap temp = new JsonSerializer().Deserialize(rd, typeof(ConfigMap)) as ConfigMap;
				if (temp != null)
					Settings = temp;
				else
					throw new IOException();
			}
		}

        //writes the config file.
        public static void WriteConfigs() {
			using (StreamWriter wr = new StreamWriter(new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite)))
				//Replace with your preffered method of serialization
				JsonSerializer.Create(new JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented }).Serialize(wr, Settings);
		}

		/// <summary>
		/// This is used to initilize any settings after thing have loaded.
		/// This is where things like databse quereys and other file reads should happen.
		/// </summary>
		private static void Initilize() {

		}

	}
}
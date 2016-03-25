using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Plugin_Base;

namespace Free_Sharp_Player {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
        //ConsoleRunner theThing = new ConsoleRunner();
        public static List<PluginBase> Plugins = new List<PluginBase>();

        [DllImport("kernel32")]
        static extern bool AllocConsole();


        public App() {
            AllocConsole();

            if (Directory.Exists("Plugins")) {
                foreach (String dir in Directory.GetDirectories("Plugins")) {
                    var dllFileNames = Directory.GetFiles(dir, "*.dll");
                    foreach (String name in dllFileNames) {
                        AssemblyName an = AssemblyName.GetAssemblyName(name);
                        Assembly assembly = Assembly.Load(an);
                        if (assembly != null) {
                            foreach (Type t in assembly.GetTypes()) {
                                if (!t.IsInterface && !t.IsAbstract && t.GetInterface("PluginBase") != null) {
                                    Plugins.Add(Activator.CreateInstance(t) as PluginBase);
                                    Console.WriteLine("Plugin Added: " + t.ToString());
                                }
                            }
                        }
                    }
                }
                if (Plugins.Count == 0)
                    Console.WriteLine("No Plugins Found");
            } else {
                Console.WriteLine("No Plugins Found");
                //TODO: popup saying no plugins found, returns here.
            }

			//theThing.Run();
		}
	}
}

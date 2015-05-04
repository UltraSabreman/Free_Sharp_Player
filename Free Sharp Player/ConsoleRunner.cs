using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	using Timer = System.Timers.Timer;
	class ConsoleRunner {

		[DllImport("kernel32")]
		static extern bool AllocConsole();
		private StreamManager streamManager;
		private Object trackLock = new Object();
		private Object radioLock = new Object();

		public ConsoleRunner() {
			AllocConsole();


		}
		public enum StreamQuality { Low, Normal, High };

		/*private void ConnectToStream(StreamQuality Quality) {
			String address = "";
			bool Connected = false;

			while (!Connected) {

				getRadioInfo temp = getRadioInfo.doPost();

				using (WebClient wb = new WebClient()) {
					var payload = new Dictionary<String, Object> {
						{"sid", temp.servers.medQuality.Split("=".ToCharArray())[1]},
					};

					String tempAddr = temp.servers.medQuality.Split("?".ToCharArray())[0];

					string[] responseData = HttpPostRequest.PostRequest(payload, tempAddr).Split("\n".ToCharArray(), StringSplitOptions.None);

					//Todo: timeout, check for valid return data, find the adress in more dynamic way.
					address = responseData[2].Split("=".ToCharArray())[1];
				}


				try {
					streamManager = new StreamManager(address);

					streamManager.NewCurrentTrack += (Track track) => {
						lock (trackLock) {
							track.Print();

							//mainModel.UpdateSong(track);
							//playlistModel.UpdateSong(track);
						}
					};

					streamManager.OnRadioUpdate += (getRadioInfo info, List<Track> played, List<Track> queued) => {
						lock (radioLock) {
							info.Print();
							//Util.PrintLine(info);

							//mainModel.UpdateInfo(info);
							//playlistModel.UpdateLists(played, queued);
							//extraModel.UpdateInfo(info);
						}
					};

					Connected = true;
				} catch (Exception) { }
			}

		}*/
		public void Run() {
			//ConnectToStream(StreamQuality.Normal);

			//streamManager.Play();

		}
	}
}

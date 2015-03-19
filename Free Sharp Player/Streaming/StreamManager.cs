using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	using Timer = System.Timers.Timer;

	class StreamManager {
		private Object theLock = new Object();

		public bool IsPlaying { get; private set; }
		public Timer mainUpdateTimer { get; private set; }
		public float Volume {
			get {
				if (theQueue != null)
					return theQueue.Volume;
				else
					return -1;
			}
			set {
				if (theQueue != null)
					theQueue.Volume = value;
			}
		}

		public delegate void TrackUpdate(Track track);
		public event TrackUpdate NewCurrentTrack;

		public delegate void RadioUpdate(getRadioInfo info, List<Track> played, List<Track> request);
		public event RadioUpdate OnRadioUpdate;


		private MP3Stream theStream;
		private StreamQueue theQueue;

		private getRadioInfo radioInfo;
		private List<Track> playedList;
		private List<Track> requested;
		private Track currentTrack;

		public StreamManager(String address) {
			playedList = new List<Track>();
			requested = new List<Track>();


			theQueue = new StreamQueue();
			theStream = new MP3Stream(address, theQueue);

			theQueue.OnStreamSongChange += UpdateData;

			mainUpdateTimer = new Timer(1000);
			mainUpdateTimer.AutoReset = true;
			mainUpdateTimer.Enabled = true;
			mainUpdateTimer.Elapsed += UpdateData;
			mainUpdateTimer.Start();

			/*
			durationUpdate = new Timer(1000);
			durationUpdate.AutoReset = true;
			durationUpdate.Enabled = true;
			durationUpdate.Elapsed += (o, e) => {
				if (theQ == null || theQ.MaxBufferLengthSec <= 0 || OnBufferChange == null) return;
				int percent = (int)Math.Round((theQ.CurBufferLengthSec / theQ.MaxBufferLengthSec) * 100);
				OnBufferChange(percent);
			};
			durationUpdate.Start();
			 */
		}

		public void ManualUpdate() {
			UpdateData(null);
		}

		private void UpdateData(Object o, EventArgs e) {
			UpdateData(null);
		}

		//TODO: potentual issues with disconnect.
		private void UpdateData(DateTime? newStart) {
			lock (theLock) {
				List<lastPlayed> playedTracks = lastPlayed.doPost();
				playedList.Clear();

				foreach (lastPlayed l in playedTracks) {
					getTrack temp = getTrack.doPost(l.trackID);
					if (temp != null && temp.track != null && temp.track.Count > 0)
						playedList.Add(temp.track[0]);
				}

				if (newStart != null) {
					currentTrack = playedList.First();
					currentTrack.localLastPlayed = (DateTime)newStart;
					if (NewCurrentTrack != null)
						NewCurrentTrack(currentTrack);
				}

				requested = getRequests.doPost().track;
				radioInfo = getRadioInfo.doPost();

				if (OnRadioUpdate != null)
					OnRadioUpdate(radioInfo, playedList, requested);
			}
		}

		public void Play() {
			if (IsPlaying) return;
			IsPlaying = true;

			theStream.Play();
			theQueue.Play();
		}

		public void Stop() {
			if (!IsPlaying) return;
			IsPlaying = false;

			theStream.Stop();
			theQueue.Stop();
		}


	}
}

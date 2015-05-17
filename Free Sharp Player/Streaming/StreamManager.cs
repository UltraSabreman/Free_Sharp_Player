using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	using Timer = System.Timers.Timer;

	public class StreamManager {
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

		public double MaxBufferSize { get { return theQueue.MaxTineInQueue; } set { theQueue.MaxTineInQueue = value; } }
		public double PlayedLegnth { get { return  theQueue.BufferedTime; } }
		public double TotalLength { get { return theQueue.TotalTimeInQueue; } }

		public delegate void TrackUpdate(Track track);
		public event TrackUpdate NewCurrentTrack;

		public delegate void RadioUpdate(getRadioInfo info, List<getLastPlayed> played, List<Track> request);
		public event RadioUpdate OnRadioUpdate;


		private MP3Stream theStream;
		private StreamQueue theQueue;

		private getRadioInfo radioInfo;
		private List<getLastPlayed> playedList;
		private List<Track> requested;
		private Track currentTrack;

		public StreamManager(String address) {
			playedList = new List<getLastPlayed>();
			requested = new List<Track>();


			theQueue = new StreamQueue();
			theStream = new MP3Stream(address, theQueue);

			theQueue.OnEventTrigger += UpdateData;

			mainUpdateTimer = new Timer(1000);
			mainUpdateTimer.AutoReset = true;
			mainUpdateTimer.Enabled = false;
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

		public List<EventTuple> GetEvents() {
			return theQueue.GetAllEvents();
		}

		public void Seek(double sec) {
			theQueue.Seek(sec);
		}

		public void ManualUpdate() {
			UpdateData();
		}

		private void UpdateData(Object o, EventArgs e) {
			UpdateData();
		}

		//TODO: potentual issues with disconnect.
		private void UpdateData(EventType type = EventType.None) {
			if (IsPlaying && theStream.EndOfStream)
				theStream.Play();

			lock (theLock) {
				//get all played tracks
				playedList.Clear();
	
				playedList = getLastPlayed.doPost();
				//update current track if nessesary
				if (type == EventType.SongChange) {
					currentTrack = getTrack.doPost(int.Parse(playedList.First().trackID)).track[0];
					currentTrack.localLastPlayed = DateTime.Now;
					if (NewCurrentTrack != null)
						NewCurrentTrack(currentTrack);
				} else if (type == EventType.Disconnect) {
					//TODO: handle disconnect.  Ui hooks?
				}

				//get list of requested tracks + radio info
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
			UpdateData(EventType.SongChange);
			mainUpdateTimer.Enabled = true;
		}

		public void Stop() {
			if (!IsPlaying) return;
			IsPlaying = false;

			theStream.Stop();
			theQueue.Stop();
			mainUpdateTimer.Enabled = false;
		}


	}
}

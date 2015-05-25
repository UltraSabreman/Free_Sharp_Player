﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	using System.Threading;

	/* Road to success:
	 * 1) Make everything event based and make seek start events when seeks poast them (get rid of all timers except the one in playedlistmodel)
	 * 2) Stream manager exposes all evetns and public functions from quue, nothing else
	 * 3) All models refernce a stream manager and subscribe to approprate aevents
	 * 4) Make song progress colors be 4, one for playing, 1 for stream title != current song, 1 for live, 1 for stopped/Dced
	 * 5) Make all ui update on eithe revents or interaction
	 * 6) Make very basic song request UI, bind played buttons to like/dislike, and very basic song info ui
	 * ----- Version Change
	 * 7) Redo UI code + XAML to match MVVM patter
	 * 8) add colors themeing + redo (if needed) request ui and info ui
	 * 9) version 1.0!
	 */

	public class StreamManager {
		private Object theLock = new Object();

		public bool IsPlaying { get; private set; }
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

		/*public delegate void TrackUpdate(Track track);
		public event TrackUpdate NewCurrentTrack;

		public delegate void RadioUpdate(getRadioInfo info, List<getLastPlayed> played, List<Track> request);
		public event RadioUpdate OnRadioUpdate;*/

		#region Wrappped Queue Events
		public delegate void EventTriggered(EventTuple e);
		public event EventTriggered OnEventTrigger;

		public delegate void Buffering(bool isBuffering);
		public event Buffering OnBufferingStateChange;

		public delegate void QueueTick(QueueSettingsTuple theStuff);
		public event QueueTick OnQueueTick;
		#endregion


		private MP3Stream theStream;
		private StreamQueue theQueue;

		/*private getRadioInfo radioInfo;
		private List<getLastPlayed> playedList;
		private List<Track> requested;
		private Track currentTrack;*/

		private String firstTrackId = null;
		private DateTime firstTrackStartTime;

		public StreamManager(String address) {
			//playedList = new List<getLastPlayed>();
			//requested = new List<Track>();

			//int maxBuff = 30, int maxQueue = 300, double minBuffer = 3, double insertSec = 0.5)
			QueueSettingsTuple tup = new QueueSettingsTuple() {
				MaxBufferedTime = 30,
				MaxTimeInQueue = 300,
				MinBufferedTime = 3,
				NumSecToPlay = 0.5
			};

			theQueue = new StreamQueue(tup);
			theStream = new MP3Stream(address, theQueue);

			//theQueue.OnEventTrigger += UpdateData;

			#region Queue Event Wrappers
			theQueue.OnEventTrigger += (e) => {
				if (OnEventTrigger != null)
					OnEventTrigger(e);
			};

			theQueue.OnBufferingStateChange += (b) => {
				if (OnBufferingStateChange != null)
					OnBufferingStateChange(b);
			};

			theQueue.OnQueueTick += (s) => {
				if (OnQueueTick != null)
					OnQueueTick(s);
			};
			#endregion
		}

		public List<EventTuple> GetEvents() {
			return theQueue.GetAllEvents();
		}

		public void Seek(double sec) {
			theQueue.Seek(sec);
		}

		//TODO: run on event
		//TODO: potentual issues with disconnect.
		/*public void UpdateData(EventTuple tup) {
			new Thread(() => {
				if (IsPlaying && theStream.EndOfStream)
					theStream.Play(); //restart stream if needed

				lock (theLock) {
					//get all played tracks
					radioInfo = getRadioInfo.doPost();
					playedList.Clear();

					playedList = getLastPlayed.doPost();
					//update current track if nessesary
					if (tup != null) {
						if (tup.Event == EventType.SongChange) {

							currentTrack = getTrack.doPost(int.Parse(playedList.First().trackID)).track[0];

							//TODO: Song Progress: Fiz timing issue
							Util.PrintLine(tup.EventQueuePosition);
							if (tup.EventQueuePosition == 0 && firstTrackId == null) {
								firstTrackId = currentTrack.trackID;
								firstTrackStartTime = DateTime.UtcNow;
							}

							if (firstTrackId != null && firstTrackId == currentTrack.trackID) {
								TimeSpan diffrence = DateTime.Parse(currentTrack.lastPlayed) - firstTrackStartTime;
								currentTrack.duration = (TimeSpan.Parse(currentTrack.duration) + diffrence + TimeSpan.FromSeconds(theQueue.BufferedTime)).ToString();
							}

							if (currentTrack.WholeTitle == radioInfo.title)
								currentTrack.lastPlayed = DateTime.UtcNow.ToString();

							if (NewCurrentTrack != null)
								NewCurrentTrack(currentTrack);
						} else if (tup.Event == EventType.Disconnect) {
							//TODO: handle disconnect.  Ui hooks?
						}
					}

					//get list of requested tracks + radio info
					requested = getRequests.doPost().track;
					

					if (OnRadioUpdate != null)
						OnRadioUpdate(radioInfo, playedList, requested);
				}
			}).Start();
		}*/

		public void Play() {
			if (IsPlaying) return;
			IsPlaying = true;

			theStream.Play();
			theQueue.Play();
			//UpdateData(new EventTuple() { Event = EventType.SongChange, EventQueuePosition = 0 });
			//mainUpdateTimer.Enabled = true;
		}

		public void Stop() {
			if (!IsPlaying) return;
			IsPlaying = false;

			theStream.Stop();
			theQueue.Stop();
			//mainUpdateTimer.Enabled = false;
		}


	}
}

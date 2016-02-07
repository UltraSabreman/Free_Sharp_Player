using System;
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

		public StreamManager(String address) {
			QueueSettingsTuple tup = new QueueSettingsTuple() {
				MaxBufferedTime = 30,
				MaxTimeInQueue = 300,
				MinBufferedTime = 3,
				NumSecToPlay = 0.5
			};

			theQueue = new StreamQueue(tup);
			theStream = new MP3Stream(address, theQueue);

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	class StreamQueue {
		public List<StreamFrame> frameQueue = new List<StreamFrame>();

		public int ReadPos { get; private set; }
		public int MaxLengthSec { get; set; }
		public int BufferMinLength { get; set; }
		//public static int MaxBufferLength = 20;

		public StreamQueue() {
			MaxLengthSec = 300;
			BufferMinLength = 4;
			ReadPos = 0;
		}

		public double GetStreamLengthMS() {
			double total = 0;
			foreach (StreamFrame f in frameQueue)
				total += f.FrameLengthMS;

			return (total / frameQueue.Count);
		}


		public void InsertFrame(StreamFrame frm) {
			if (GetStreamLengthMS() * 1000 > MaxLengthSec)
				frameQueue.RemoveAt(0);

			frameQueue.Add(frm);
		}

		public StreamFrame ReadFrame() {
			double len = GetStreamLengthMS() * 1000;

			if (len < BufferMinLength) return null;

			return frameQueue[ReadPos];
		}
	}
}

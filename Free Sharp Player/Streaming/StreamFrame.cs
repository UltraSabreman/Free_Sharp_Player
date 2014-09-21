using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using Newtonsoft.Json;

namespace Free_Sharp_Player {
	class StreamFrame {
		private static String OldTitle = null;

		private static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame frame) {
			return new AcmMp3FrameDecompressor(new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2, frame.FrameLength, frame.BitRate));
		}

		public Mp3Frame CurrentFrame { get; set; }
		public String CurrentTitle { get; private set; }
		public Track CurrentTrack { get; set; }
		private PlaylistModel playlist;

		public StreamFrame(Mp3Frame frame, PlaylistModel pl, String title) {
			CurrentFrame = frame;
			playlist = pl;
			CurrentTrack = playlist.Playing;

			CurrentTitle = title;

			if (OldTitle != title)
				CurrentTrack = playlist.Playing;
		}

		public double FrameLengthMS {
			get {
				return (CurrentFrame.SampleCount / CurrentFrame.SampleRate) * 1000;
			}
		}
		

		public byte[] Decompress() {
			IMp3FrameDecompressor decomp = CreateFrameDecompressor(CurrentFrame);
			Byte[] decompressedFrame = new Byte[CurrentFrame.FrameLength];


			int decompressed = decomp.DecompressFrame(CurrentFrame, decompressedFrame, 0);

			if (decompressed <= 0) return null;
			return decompressedFrame;
		}
	}
}

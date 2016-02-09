using System;
using System.Collections.Generic;

namespace Plugin_Base {
    public enum Quality { Low = 3, Normal = 1, High = 2 };
    public enum Vote { Down, Netural, Up };

    public interface BasePlugin {
        void Initilize(Quality StreamQuality = Quality.Normal);
        PluginInfo GetPluginCapabilities();
        StreamStatus GetStreamInformation();

        List<Track> GetPlayedTracks();
        List<Track> GetFutureTracks();
        Track GetCurrentTrack();
        void RequestTrack(Track TrackToRequest);
        void VoteOnTrack(Track TrackToVoteOne, Vote TrackVote);

    }

    public interface PluginInfo {
        bool CanFetchPlayedTracks { get; set; }
        bool CanFetchFutureTracks { get; set; }
        bool CanFetchCurrentTrack { get; set; }
        bool CanRequestTracks { get; set; }
        bool CanVoteOnTracks { get; set; }
    }

    public interface StreamStatus {
        bool IsUp { get; set; }
        bool IsLive { get; set; }
        String StreamAddress { get; set; }
        String StreamTitle { get; set; }
        int Listeners { get; set; }
    }

    public interface Track {
        String TrackID { get; set; }
        String Title { get; set; }
        String Artist { get; set; }
        int DurationSec { get; set; }
        int Rating { get; set; }
    }
}

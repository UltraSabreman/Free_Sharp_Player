using System;
using System.Collections.Generic;

namespace Plugin_Base {
    public enum Quality {None, Low, Medium, High };
    public enum Vote { Down, Netural, Up };

    public interface PluginBase {
        PluginInfo GetPluginCapabilities();
        StreamStatus GetStreamInformation(Quality StreamQuality = Quality.Medium);

        List<Track> GetPlayedTracks();
        List<Track> GetFutureTracks();
        Track GetCurrentTrack();
        void RequestTrack(Track TrackToRequest);
        void VoteOnTrack(Vote TrackVote, Track TrackToVoteOne);

    }

    public interface PluginInfo {
        bool CanFetchPlayedTracks { get; set; }
        bool CanFetchFutureTracks { get; set; }
        bool CanFetchCurrentTrack { get; set; }
        bool CanRequestTracks { get; set; }
        bool CanVoteOnCurrentTrack { get; set; }
        bool CanVoteOnAnyTrack { get; set; }
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
        String Requester { get; set; }
        int DurationSec { get; set; }
        int Rating { get; set; }
        int Plays { get; set; }
        int Requests { get; set; }
        DateTime LastPlayed { get; set; }
        DateTime RequestedTime { get; set; }

    }
}

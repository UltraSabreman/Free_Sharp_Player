using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin_Base;

namespace Plugin_Canterlot_Hill {
    class APICalls : BaseAPICalls  {

        public static SetVote SetVoteCall(int Vote, String TrackID = null) {
            var payload = new Dictionary<String, String>() {
                {"action", "setVote" },
                {"modifier",  Uri.EscapeUriString(Vote.ToString())}
            };


            if (!String.IsNullOrEmpty(TrackID))
                payload["trackID"] = Uri.EscapeUriString(TrackID);

            return HTTPGet<SetVote>(payload);
        }


        public static SetRequests SetRequestsCall(String trackID) {
            var payload = new Dictionary<String, String> {
                {"action", "setRequest"},
                {"trackID", Uri.EscapeUriString(trackID)}
            };

            return HTTPGet<SetRequests>(payload);
        }

        public static GetVoteStatus GetVoteStatusCall(String trackid = null) {
            var payload = new Dictionary<String, String> {
                {"action", "getVoteStatus"},
            };

            if (!String.IsNullOrEmpty(trackid)) payload["trackID"] = Uri.EscapeUriString(trackid);

            return HTTPGet<GetVoteStatus>(payload);
        }

        public static GetTracks GetTracksCall(String trackId, String track, String artist, int? rating, String ratingEq, int page = 1, int limit = 20) {
            var payload = new Dictionary<String, String> {
                {"action", "getTrack"},
                {"page", Uri.EscapeUriString(page.ToString())},
                {"limit", Uri.EscapeUriString(limit.ToString())},
            };

            if (trackId != null)
                payload["trackid"] = Uri.EscapeUriString(trackId);
            else {
                if (track == null && artist == null && rating == null)
                    throw new ArgumentNullException("Must provide either artist, rating, or track");

                if (track != null) payload["track"] = Uri.EscapeUriString(track);
                if (artist != null) payload["artist"] = Uri.EscapeUriString(artist);
                if (rating != null) {
                    payload["rating"] = Uri.EscapeUriString(rating.ToString());
                    if (ratingEq == null) throw new ArgumentNullException("Must provide ratingEq if spesifing rating.");
                    payload["rating_inequality"] = Uri.EscapeUriString(ratingEq);
                }
            }

            return HTTPGet<GetTracks>(payload);
        }


        public static GetRequests GetRequestsCall() {
            var payload = new Dictionary<String, String> {
                {"action", "getRequest"},
            };

            return HTTPGet<GetRequests>(payload);
        }

        public static GetRadioInfo GetRadioInfoCall() {
            var payload = new Dictionary<String, String> {
                {"action", "getRadioInfo"},
            };

            return HTTPGet<GetRadioInfo>(payload);
        }

        public static List<GetLastPlayed> doPost() {
            var payload = new Dictionary<String, String> {
                {"action", "getLastPlayed"},
            };

            var temp = HTTPGet<List<GetLastPlayed>>(payload);

            foreach (var l in temp) {
                TimeZoneInfo eastern = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                l.last_played = TimeZoneInfo.ConvertTime(DateTime.Parse(l.last_played), eastern, TimeZoneInfo.Local).ToString();
            }

            return temp;
        }
    }
}

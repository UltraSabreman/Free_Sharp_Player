using System;
using System.Collections.Generic;
using Plugin_Base;
using System.Collections.Specialized;
using System.Net;

/*
 * Long as fuck todo list to get this shit to work:
 * Make a generic request for all stations under the ponyvile live umpbrella. Can do more with this later, for now we just need the shortcode.
 *  - Needs seperate ui
 *  - Must rember choice for future launches
 *  - Must have a "bypass this dialog and go to station" check box. 
 *  - Must be able to bring up this dialog normally
 * Convert pluging to fetch information from ponyville live API
 *  - Do some research to find out if there are hidden API calls that are dissabled. I remember there being some (**DOCUMENT THIS**)
 * Figure out a good way to "dispose" of request UI/code untill the API is avalible for it.
 */

namespace Plugin {
    class PluginPonyvilleFM : PluginBase {
        private CanterlotStreamStatus Status = new CanterlotStreamStatus();
        private Quality streamQuality = Quality.None;

        public PluginPonyvilleFM() {
            APICalls.Address = "http://ponyvillelive.com/api/";
        }

        public PluginInfo GetPluginCapabilities() {
            return new CanterlotPluginInfo() {
                CanFetchCurrentTrack = false,
                CanFetchFutureTracks = false,
                CanFetchPlayedTracks = true,
                CanRequestTracks = false,
                CanVoteOnCurrentTrack = false,
                CanVoteOnAnyTrack = false
            };
        }

        public StreamStatus GetStreamInformation(Quality StreamQuality = Quality.Medium) {
            GetRadioInfo temp = APICalls.GetRadioInfoCall();

            if (StreamQuality != streamQuality) {
                streamQuality = StreamQuality;

                using (WebClient wb = new WebClient()) {
                    NameValueCollection data = new NameValueCollection();


                    int actualQuality = 0;
                    if (streamQuality == Quality.Medium)
                        actualQuality = 3;
                    else if (streamQuality == Quality.Medium)
                        actualQuality = 1;
                    else if (streamQuality == Quality.High)
                        actualQuality = 2;

                    String tempAddr = temp.servers.medQuality.Split("?".ToCharArray())[0];
                    data["sid"] = actualQuality.ToString();// temp.servers.medQuality.Split("=".ToCharArray())[1];

                    Byte[] response = wb.UploadValues(tempAddr, "POST", data);

                    string[] responseData = System.Text.Encoding.UTF8.GetString(response, 0, response.Length).Split("\n".ToCharArray(), StringSplitOptions.None);

                    //Todo: timeout, check for valid return data, find the adress in more dynamic way.
                    Status.StreamAddress = responseData[2].Split("=".ToCharArray())[1];
                    Status.StreamAddress = "http://192.99.131.205:8000/stream.mp3";
                }

            }

            //temp = APICalls.GetRadioInfoCall();
            Status.IsLive = true;// temp.autoDJ == "0";
            Status.Listeners = 1337;// Int32.Parse(temp.listeners);
            Status.StreamTitle = "Somebody once told me, the world was gonna roll me";// temp.title;
            Status.Rating = 69;// Int32.Parse(temp.rating);
            Status.IsUp = true;// temp.up;

            return Status;
        }



        public Track GetCurrentTrack() {
            throw new NotImplementedException();
        }

        public List<Track> GetFutureTracks() {
            throw new NotImplementedException();
        }

        public List<Track> GetPlayedTracks() {
            List<GetLastPlayed>  trackList = APICalls.GetPlayedTracksCall();
            List<CanterlotTrack> retTrackList = new List<CanterlotTrack>();
            foreach (GetLastPlayed temp in trackList) {
                retTrackList.Add(new CanterlotTrack() {
                    Artist = temp.artist,
                    Title = temp.title,
                    //DurationSec = temp.
                });
            }
            return null;
        }


        public void RequestTrack(Track TrackToRequest) {
            throw new NotImplementedException();
        }

        public void VoteOnTrack(Vote TrackVote, Track TrackToVoteOne) {
            throw new NotImplementedException();
        }
    }
    public class CanterlotPluginInfo : PluginInfo {
        public bool CanFetchPlayedTracks { get; set; }
        public bool CanFetchFutureTracks { get; set; }
        public bool CanFetchCurrentTrack { get; set; }
        public bool CanRequestTracks { get; set; }
        public bool CanVoteOnCurrentTrack { get; set; }
        public bool CanVoteOnAnyTrack { get; set; }
    }

    public class CanterlotStreamStatus : StreamStatus {
        public bool IsUp { get; set; }
        public bool IsLive { get; set; }
        public String StreamAddress { get; set; }
        public String StreamTitle { get; set; }
        public int Listeners { get; set; }
        //TODO: do i want this here?
        public int Rating { get; set; }

    }
}

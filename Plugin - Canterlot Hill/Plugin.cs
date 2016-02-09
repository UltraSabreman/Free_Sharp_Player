using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin_Base;
using System.Collections.Specialized;
using System.Net;

namespace Plugin_Canterlot_Hill {
    class Plugin : BasePlugin {
        private String Address = "http://api.canterlothill.com/v1/";
        private CanterlotStreamStatus Status = new CanterlotStreamStatus();

        public PluginInfo GetPluginCapabilities() {
            return new CanterlotPluginInfo() {
                CanFetchCurrentTrack = true,
                CanFetchFutureTracks = false,
                CanFetchPlayedTracks = false,
                CanRequestTracks = false,
                CanVoteOnTracks = false
            };
        }

        public void Initilize(Quality StreamQuality = Quality.Normal) {
            APICalls.Address = Address;

            GetRadioInfo temp = APICalls.GetRadioInfoCall();
            Status.IsLive = temp.autoDJ == "0";
            Status.Listeners = Int32.Parse(temp.listeners);
            Status.StreamTitle = temp.title;
            Status.Rating = Int32.Parse(temp.rating);
            Status.IsUp = temp.up;


            using (WebClient wb = new WebClient()) {
                NameValueCollection data = new NameValueCollection();
                String tempAddr = temp.servers.medQuality.Split("?".ToCharArray())[0];
                data["sid"] = temp.servers.medQuality.Split("=".ToCharArray())[(int)StreamQuality];

                Byte[] response = wb.UploadValues(tempAddr, "POST", data);

                string[] responseData = System.Text.Encoding.UTF8.GetString(response, 0, response.Length).Split("\n".ToCharArray(), StringSplitOptions.None);

                //Todo: timeout, check for valid return data, find the adress in more dynamic way.
                Status.StreamAddress = responseData[2].Split("=".ToCharArray())[1];
            }

        }


        public StreamStatus GetStreamInformation() {
            GetRadioInfo temp = APICalls.GetRadioInfoCall();
            Status.IsLive = temp.autoDJ == "0";
            Status.Listeners = Int32.Parse(temp.listeners);
            Status.StreamTitle = temp.title;
            Status.Rating = Int32.Parse(temp.rating);
            Status.IsUp = temp.up;
            return Status;
        }



        public Track GetCurrentTrack() {
            throw new NotImplementedException();
        }

        public List<Track> GetFutureTracks() {
            throw new NotImplementedException();
        }

        public List<Track> GetPlayedTracks() {
            throw new NotImplementedException();
        }


        public void RequestTrack(Track TrackToRequest) {
            throw new NotImplementedException();
        }

        public void VoteOnTrack(Track TrackToVoteOne, Vote TrackVote) {
            throw new NotImplementedException();
        }
    }
    public class CanterlotPluginInfo : PluginInfo {
        public bool CanFetchPlayedTracks { get; set; }
        public bool CanFetchFutureTracks { get; set; }
        public bool CanFetchCurrentTrack { get; set; }
        public bool CanRequestTracks { get; set; }
        public bool CanVoteOnTracks { get; set; }
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

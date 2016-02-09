using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Base {
    public class BaseAPICalls {
        public static String Address = "";

        protected static T HTTPGet<T>(Dictionary<String, String> Payload) where T : class {
            //TODO: is this to complicated?
            String result = HttpPostRequest.PostRequest(Payload, Address);

            return JsonConvert.DeserializeObject(StringToDict(result)["data"], typeof(T)) as T;
        }

        protected static Dictionary<String, String> StringToDict(String msg) {
            if (msg == null) return null;

            var outd = new Dictionary<String, String>();

            var test = JsonConvert.DeserializeObject(msg) as JObject;
            foreach (JProperty p in test.Properties())
                outd[p.Name] = p.Value.ToString();


            return outd;
        }

    }
}

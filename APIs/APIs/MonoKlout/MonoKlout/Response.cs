using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace MonoKlout
{
    internal class Response
    {
        internal static KloutIdentityResponse MakeIdentityRequest(string requestUrl)
        {
            #if DEBUG
                Console.WriteLine("Inside MakeIdentityRequest().");
                Console.WriteLine("Request: {0}", requestUrl);
            #endif

            try
            {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(string.Format("Server error {0} : {1}",
                                            response.StatusCode,
                                            response.StatusDescription));
                    }

                    var reader = new StreamReader(response.GetResponseStream());
                    string json = reader.ReadToEnd();

                    object deserializedObj = JsonConvert.DeserializeObject<KloutIdentityResponse>(json);
                    KloutIdentityResponse identityResponse = deserializedObj as KloutIdentityResponse;

                    return identityResponse;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        internal static T MakeRequest<T>(string requestUrl)
        {
            #if DEBUG
                Console.WriteLine("Inside MakeRequest().");
                Console.WriteLine("Request: {0}", requestUrl);
            #endif
            
            try
            {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(string.Format("Server error {0} : {1}",
                                            response.StatusCode,
                                            response.StatusDescription));
                    }

                    var reader = new StreamReader(response.GetResponseStream());
                    string json = reader.ReadToEnd();

                    object deserializedObj = JsonConvert.DeserializeObject<T>(json);
                    T responseObj = (T)deserializedObj;
                    
                    return responseObj;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return default(T);
            }
        }
    }
}


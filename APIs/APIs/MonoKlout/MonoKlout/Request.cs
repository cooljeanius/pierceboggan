using System;

namespace MonoKlout
{
    /*
     * The purpose of this class is to generate the appropriate request for the user's need.
     */
    internal class Requests
    {
        // Base request Urls
        const string baseRequestUrl = "http://api.klout.com/v2/user.json/";
        const string initialRequestUrl = "http://api.klout.com/v2/identity.json/twitter?screenName=";
        const string apiKeyUrl = "?key=";

        // Extension Urls
        public static string scoreRequestExtension = "/score";
        public static string influenceRequestExtension = "/influence";
        public static string userTopicsRequestExtension = "/topics";

        internal static string GenerateIdentityRequest(string twitterUsername)
        {
            #if DEBUG
                Console.WriteLine("Inside GenerateIdentityRequest().");
                Console.WriteLine("Generating Identity Request for {0}.", twitterUsername);
            #endif

            string request = initialRequestUrl + twitterUsername + "&key=" + KloutAPI.APIKey;

            return request;
        }

        internal static string GenerateRequest(string kloutId, string requestExtension)
        {
            #if DEBUG
                Console.WriteLine("Inside GenerateRequest().");
                Console.WriteLine("Generating Request for {0}.", kloutId);
                Console.WriteLine("Request Extension: {0}", requestExtension);
            #endif

            string request = baseRequestUrl + kloutId + requestExtension + apiKeyUrl + KloutAPI.APIKey;

            return request;
        }
    }
}

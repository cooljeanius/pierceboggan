using System;
using Newtonsoft.Json;

namespace MonoKlout
{
    /*
     * This class is used as a model for deserializing the Json returned by the HttpWebRequest.
     */
    [JsonObject(MemberSerialization.OptIn)]
    public class KloutIdentityResponse
    {
        [JsonProperty("id")]
        public string id { get; set; }
        
        [JsonProperty("network")]
        public string network { get; set; }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class KloutScoreResponse
    {
        [JsonProperty("score")]
        public string score { get; set; }
        
        [JsonProperty("scoreDelta")]
        public KloutScoreDeltaResponse scoreDelta { get; set; }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class KloutScoreDeltaResponse
    {
        [JsonProperty("dayChange")]
        public string dayChange { get; set; }
        
        [JsonProperty("weekChange")]
        public string weekChange { get; set; }
        
        [JsonProperty("monthChange")]
        public string monthChange { get; set; }
    }

    public class KloutUserTopicsResponse
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("displayName")]
        public string displayName { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("slug")]
        public string slug { get; set; }

        [JsonProperty("imageUrl")]
        public string imageUrl { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class KloutInfluenceResponse
    {
        [JsonProperty("myInfluencers")]
        public KloutInfluenceEntityResponse[] myInfluencers { get; set; }

        [JsonProperty("myInfluencees")]
        public KloutInfluenceEntityResponse[] myInfluencees { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class KloutInfluenceEntityResponse
    {
        [JsonProperty("entity")]
        public KloutInfluenceEntityDetailedResponse entity { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class KloutInfluenceEntityDetailedResponse
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("payload")]
        public KloutInfluencePayloadResponse payload { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class KloutInfluencePayloadResponse
    {
        [JsonProperty("kloutId")]
        public string kloutId { get; set; }

        [JsonProperty("nick")]
        public string nick { get; set; }

        [JsonProperty("score")]
        public KloutInfluenceScoreResponse score { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class KloutInfluenceScoreResponse
    {
        [JsonProperty("score")]
        public string score { get; set; }
    }
}  
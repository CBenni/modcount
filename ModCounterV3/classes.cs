using System.Collections.Generic;
namespace FollowClasses
{
    public class Links
    {
        public string self { get; set; }
        public string next { get; set; }
    }

    public class Links2
    {
        public string self { get; set; }
    }

    public class Links3
    {
        public string self { get; set; }
        public string follows { get; set; }
        public string commercial { get; set; }
        public string stream_key { get; set; }
        public string chat { get; set; }
        public string features { get; set; }
        public string subscriptions { get; set; }
        public string editors { get; set; }
        public string teams { get; set; }
        public string videos { get; set; }
    }

    public class Channel
    {
        public bool? mature { get; set; }
        public object abuse_reported { get; set; }
        public string status { get; set; }
        public string display_name { get; set; }
        public string game { get; set; }
        public int delay { get; set; }
        public int _id { get; set; }
        public string name { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string primary_team_name { get; set; }
        public string primary_team_display_name { get; set; }
        public string logo { get; set; }
        public string banner { get; set; }
        public string video_banner { get; set; }
        public string background { get; set; }
        public string profile_banner { get; set; }
        public string profile_banner_background_color { get; set; }
        public string url { get; set; }
        public long views { get; set; }
        public long followers { get; set; }
        public Links3 _links { get; set; }
    }

    public class Follow
    {
        public string created_at { get; set; }
        public Links2 _links { get; set; }
        public Channel channel { get; set; }
    }

    public class RootObject
    {
        public int _total { get; set; }
        public Links _links { get; set; }
        public List<Follow> follows { get; set; }
    }
}

namespace AuthClasses
{
    public class Authorization
    {
        public List<string> scopes { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
    }

    public class Token
    {
        public Authorization authorization { get; set; }
        public string user_name { get; set; }
        public bool valid { get; set; }
    }

    public class Links
    {
        public string channel { get; set; }
        public string users { get; set; }
        public string user { get; set; }
        public string channels { get; set; }
        public string chat { get; set; }
        public string streams { get; set; }
        public string ingests { get; set; }
    }

    public class RootObject
    {
        public Token token { get; set; }
        public Links _links { get; set; }
    }
}
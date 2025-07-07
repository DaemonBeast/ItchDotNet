namespace ItchDotNet;

public static class Constants
{
    public static class Endpoints
    {
        public static class Itch
        {
            public static class Api
            {
                public const string V1 = "https://itch.io/api/1";
                public const string V2 = "https://api.itch.io";

                public static class Login
                {
                    public const string WithPassword = V2 + "/login";
                }

                public static class Profile
                {
                    public const string OwnedKeys = V2 + "/profile/owned-keys";
                }

                public static class Uploads
                {
                    public const string Upload = V2 + "/uploads/{0}";
                    public const string Build = V2 + "/builds/{0}";
                    public const string Builds = V2 + "/uploads/{0}/builds";
                }
            }
        }
    }
}

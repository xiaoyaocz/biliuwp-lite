namespace BiliLite.Models.Responses
{
    public class LoginAppThirdResponse
    {
        public string api_host { get; set; }
        public int has_login { get; set; }
        public int direct_login { get; set; }
        public string confirm_uri { get; set; }
    }
}

//namespace PolyhydraGames.Twitch.Services
//{
//    public class WrappedTwitchApi
//    {
//        private readonly TwitchAPI _api;
//        private readonly IApiSettings _apiconfig;
//        private readonly ILogger<WrappedTwitchApi> _log;
//        private string _token;

//        private void SetToken(string value)
//        {
//            _token = value;
//        }
//        public WrappedTwitchApi(TwitchAPI api,
//            IApiSettings apiConfig,
//            ILogger<WrappedTwitchApi> log)
//        {
//            _api = api;
//            _apiconfig = apiConfig;
//            _log = log;
//        }

//        public Task<GetUsersResponse> GetStreamerDetails(string username)
//        {
//            return GetStreamerDetails(null, [username]);
//        }

//        public Task<GetUsersResponse> GetStreamerDetails(List<string>? ids = null, List<string>? usernames = null)
//        {
//            return _api.Helix.Users.GetUsersAsync(ids, usernames, _token);
//        }
//    }
//}
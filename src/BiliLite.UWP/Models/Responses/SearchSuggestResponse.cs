using System.Collections.Generic;

namespace BiliLite.Models.Responses
{
    public class SearchSuggestResponse
    {
        public List<SearchSuggestResponseTag> Tag { get; set; }
    }

    public class SearchSuggestResponseTag
    {
        public string Value { get; set; }
    }
}

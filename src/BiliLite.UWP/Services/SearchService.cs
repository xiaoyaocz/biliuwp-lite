using BiliLite.Extensions;
using BiliLite.Models.Requests.Api;
using BiliLite.Models.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiliLite.Services
{
    public class SearchService
    {
        public async Task<List<string>> GetSearchSuggestContents(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var suggestSearchResult = await new SearchAPI().SearchSuggest(text).Request();
            if (!suggestSearchResult.status) return null;
            var suggestSearch = await suggestSearchResult.GetResult<SearchSuggestResponse>();
            if (suggestSearch == null) return null;
            if (suggestSearch.result == null) return null;
            var suggestSearchContent = suggestSearch.result.Tag.Select(x => x.Value);
            return suggestSearchContent.ToList();
        }
    }
}

using BiliLite.Models;
using BiliLite.Models.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace BiliLite.Extensions
{
    public static class HttpResultsExtensions
    {
        public static async Task<T> GetJson<T>(this HttpResults httpResults)
        {
            return await Task.Run<T>(() =>
            {
                return JsonConvert.DeserializeObject<T>(httpResults.results);
            });
        }

        public static JObject GetJObject(this HttpResults httpResults)
        {
            try
            {
                var obj = JObject.Parse(httpResults.results);
                return obj;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<ApiDataModel<T>> GetData<T>(this HttpResults httpResults)
        {
            try
            {
                return await GetJson<ApiDataModel<T>>(httpResults);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<ApiResultModel<T>> GetResult<T>(this HttpResults httpResults)
        {
            try
            {
                return await GetJson<ApiResultModel<T>>(httpResults);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

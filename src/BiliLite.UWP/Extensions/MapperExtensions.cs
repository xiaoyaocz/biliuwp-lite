using AutoMapper;
using BiliLite.Models.Common.Comment;
using BiliLite.Models.Download;
using BiliLite.ViewModels.Comment;
using BiliLite.ViewModels.Download;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Extensions
{
    public static class MapperExtensions
    {
        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            var mapper = new Mapper(new MapperConfiguration(expression =>
            {
                expression.CreateMap<DownloadItem, DownloadItemViewModel>();
                expression.CreateMap<DownloadItemViewModel, DownloadItem>();
                expression.CreateMap<DownloadEpisodeItem, DownloadEpisodeItemViewModel>();
                expression.CreateMap<DownloadEpisodeItemViewModel, DownloadEpisodeItem>();
                expression.CreateMap<CommentItem, CommentViewModel>();
                expression.CreateMap<CommentViewModel, CommentItem>();
                expression.CreateMap<DataCommentModel, DataCommentViewModel>();
                expression.CreateMap<DataCommentViewModel, DataCommentModel>();
            }));
            services.AddSingleton<IMapper>(mapper);
            return services;
        }
    }
}

using AutoMapper;
using BiliLite.Models.Common.Comment;
using BiliLite.Models.Common.Video.Detail;
using BiliLite.Models.Download;
using BiliLite.ViewModels.Comment;
using BiliLite.ViewModels.Download;
using BiliLite.ViewModels.Video;
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
                expression.CreateMap<VideoDetailViewModel, VideoDetailModel>();
                expression.CreateMap<VideoDetailModel, VideoDetailViewModel>(); 
                expression.CreateMap<VideoDetailStaffViewModel, VideoDetailStaffModel>(); 
                expression.CreateMap<VideoDetailStaffModel, VideoDetailStaffViewModel>();
                expression.CreateMap<VideoDetailStatViewModel, VideoDetailStatModel>();
                expression.CreateMap<VideoDetailStatModel, VideoDetailStatViewModel>();
                expression.CreateMap<VideoDetailRelatesViewModel, VideoDetailRelatesModel>();
                expression.CreateMap<VideoDetailRelatesModel, VideoDetailRelatesViewModel>();
                expression.CreateMap<VideoDetailReqUserViewModel, VideoDetailReqUserModel>();
                expression.CreateMap<VideoDetailReqUserModel, VideoDetailReqUserViewModel>();
            }));
            services.AddSingleton<IMapper>(mapper);
            return services;
        }
    }
}

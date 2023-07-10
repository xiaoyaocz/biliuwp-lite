using System;
using AutoMapper;
using BiliLite.Models.Common.Comment;
using BiliLite.Models.Common.Season;
using BiliLite.Models.Common.Video.Detail;
using BiliLite.Models.Download;
using BiliLite.ViewModels.Comment;
using BiliLite.ViewModels.Download;
using BiliLite.ViewModels.Season;
using BiliLite.ViewModels.Video;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;

namespace BiliLite.Extensions
{
    public static class MapperExtensions
    {
        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            var mapper = new Mapper(new MapperConfiguration(expression =>
            {
                expression.CreateMap<DownloadItem, DownloadItemViewModel>();
                expression.CreateMap<DownloadEpisodeItem, DownloadEpisodeItemViewModel>();
                expression.CreateMap<CommentItem, CommentViewModel>();
                expression.CreateMap<DataCommentModel, DataCommentViewModel>();
                expression.CreateMap<CommentContentModel, CommentContentViewModel>();
                expression.CreateMap<VideoDetailModel, VideoDetailViewModel>(); 
                expression.CreateMap<VideoDetailStaffModel, VideoDetailStaffViewModel>();
                expression.CreateMap<VideoDetailStatModel, VideoDetailStatViewModel>();
                expression.CreateMap<VideoDetailRelatesModel, VideoDetailRelatesViewModel>();
                expression.CreateMap<VideoDetailReqUserModel, VideoDetailReqUserViewModel>();
                expression.CreateMap<SeasonDetailUserStatusModel, SeasonDetailUserStatusViewModel>();
                expression.CreateMap<SeasonDetailModel, SeasonDetailViewModel>();
            }));
            services.AddSingleton<IMapper>(mapper);
            return services;
        }
    }
}

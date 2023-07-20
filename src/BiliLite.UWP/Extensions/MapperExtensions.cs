using AutoMapper;
using Bilibili.App.Interface.V1;
using BiliLite.Models.Common.Comment;
using BiliLite.Models.Common.Season;
using BiliLite.Models.Common.User;
using BiliLite.Models.Common.Video.Detail;
using BiliLite.Models.Download;
using BiliLite.ViewModels.Comment;
using BiliLite.ViewModels.Download;
using BiliLite.ViewModels.Season;
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

                expression.CreateMap<Arc, SubmitVideoItemModel>()
                    .ForMember(dest => dest.Play, opt => opt.MapFrom(src => src.Archive.Stat.View))
                    .ForMember(dest => dest.Pic, opt => opt.MapFrom(src => src.Archive.Pic))
                    .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Archive.Title.Replace("<em class=\"keyword\">", "").Replace("</em>", "")))
                    .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Archive.Duration))
                    .ForMember(dest => dest.Aid, opt => opt.MapFrom(src => src.Archive.Aid))
                    .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Archive.Ctime))
                    .ForMember(dest => dest.VideoReview, opt => opt.MapFrom(src => src.Archive.Stat.Danmaku))
                    .ForMember(dest => dest.RedirectUrl, opt => opt.MapFrom(src => src.Archive.RedirectUrl));

                expression.CreateMap<SubmitVideoCursorItem, SubmitVideoItemModel>()
                    .ForMember(dest => dest.Pic, opt => opt.MapFrom(src => src.Cover))
                    .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Duration))
                    .ForMember(dest => dest.Aid, opt => opt.MapFrom(src => src.Aid))
                    .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.CTime))
                    .ForMember(dest => dest.VideoReview, opt => opt.MapFrom(src => src.Danmaku));
            }));
            services.AddSingleton<IMapper>(mapper);
            return services;
        }
    }
}

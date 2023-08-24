using System.Collections.Generic;
using System.Linq;
using Atelier39;
using AutoMapper;
using Bilibili.App.Dynamic.V2;
using Bilibili.App.Interface.V1;
using Bilibili.Tv.Interfaces.Dm.V1;
using BiliLite.Models.Common.Anime;
using BiliLite.Models.Common.Comment;
using BiliLite.Models.Common.Dynamic;
using BiliLite.Models.Common.Season;
using BiliLite.Models.Common.User;
using BiliLite.Models.Common.Video.Detail;
using BiliLite.Models.Download;
using BiliLite.ViewModels.Comment;
using BiliLite.ViewModels.Download;
using BiliLite.ViewModels.Home;
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
                expression.CreateMap<AnimeFallModel, AnimeFallViewModel>();

                expression.CreateMap<Arc, SubmitVideoItemModel>()
                    .ForMember(dest => dest.Play, opt => opt.MapFrom(src => src.Archive.Stat.View))
                    .ForMember(dest => dest.Pic, opt => opt.MapFrom(src => src.Archive.Pic))
                    .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Archive.Title.Replace("<em class=\"keyword\">", "").Replace("</em>", "")))
                    .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Archive.Duration.ProgressToTime()))
                    .ForMember(dest => dest.Aid, opt => opt.MapFrom(src => src.Archive.Aid))
                    .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Archive.Ctime))
                    .ForMember(dest => dest.VideoReview, opt => opt.MapFrom(src => src.Archive.Stat.Danmaku))
                    .ForMember(dest => dest.RedirectUrl, opt => opt.MapFrom(src => src.Archive.RedirectUrl));

                expression.CreateMap<SubmitVideoCursorItem, SubmitVideoItemModel>()
                    .ForMember(dest => dest.Pic, opt => opt.MapFrom(src => src.Cover))
                    .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Duration.ProgressToTime()))
                    .ForMember(dest => dest.Aid, opt => opt.MapFrom(src => src.Aid))
                    .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.CTime))
                    .ForMember(dest => dest.VideoReview, opt => opt.MapFrom(src => src.Danmaku));
            }));
            services.AddSingleton<IMapper>(mapper);
            return services;
        }

        public static List<DanmakuItem> MapToDanmakuItems(this IEnumerable<DanmakuElem> elems)
        {
            var danmakuItems = new List<DanmakuItem>();
            foreach (var danmakuElem in elems)
            {
                var danmakuItem = new DanmakuItem()
                {
                    Id = (ulong)danmakuElem.Id,
                    Text = danmakuElem.Content,
                    StartMs = (uint)danmakuElem.Progress,
                    BaseFontSize = danmakuElem.Fontsize,
                    Mode = (DanmakuMode)danmakuElem.Mode,
                    TextColor = danmakuElem.Color.ParseColor(),
                    Weight = danmakuElem.Weight,
                    MidHash = danmakuElem.MidHash
                };
                danmakuItem.ParseAdvanceDanmaku();
                danmakuItems.Add(danmakuItem);
            }
            return danmakuItems.OrderBy(x => x.StartMs).ToList();
        }

        public static List<DynamicItemModel> MapToDynamicItemModels(this IEnumerable<DynamicItem> dynamicItems)
        {
            var dynamicItemModels = new List<DynamicItemModel>();
            foreach (var src in dynamicItems)
            {
                var type = 0;
                switch (src.CardType)
                {
                    case DynamicType.Av:
                        type = 8;
                        break;
                    case DynamicType.Pgc:
                        type = 512;
                        break;
                }

                var moduleAuthor = src.Modules.FirstOrDefault(x => x.ModuleType == DynModuleType.ModuleAuthor);
                var moduleDynamic = src.Modules.FirstOrDefault(x => x.ModuleType == DynModuleType.ModuleDynamic);

                var dynDesc = new DynamicDescModel()
                {
                    Type = type,
                    DynamicId = src.Extend.DynIdStr,
                    TimeText = moduleAuthor.ModuleAuthor.PtimeLabelText
                };
                var dynItemModel = new DynamicItemModel()
                {
                    Desc = dynDesc,
                };
                switch (type)
                {
                    case 8:
                        {
                            var dynVideoStat = new DynamicVideoCardStatModel()
                            {
                                View = moduleDynamic.ModuleDynamic.DynArchive.View
                            };
                            var dynOwner = new DynamicVideoCardOwnerModel()
                            {
                                Face = src.Extend.OrigFace,
                                Name = src.Extend.OrigName,
                            };
                            var dynVideo = new DynamicVideoCardModel()
                            {
                                Aid = moduleDynamic.ModuleDynamic
                                    .DynArchive.Avid.ToString(),
                                Duration = moduleDynamic.ModuleDynamic.DynArchive.Duration,
                                Pic = src.Extend.OrigImgUrl,
                                Title = moduleDynamic.ModuleDynamic.DynArchive.Title,
                                Stat = dynVideoStat,
                                Owner = dynOwner,
                                SeasonId = moduleDynamic.ModuleDynamic.DynArchive.PgcSeasonId,
                                ViewCountText = moduleDynamic.ModuleDynamic.DynArchive.CoverLeftText2,
                                DanmakuCountText = moduleDynamic.ModuleDynamic.DynArchive.CoverLeftText3,
                            };
                            if (string.IsNullOrEmpty(dynVideo.Title))
                            {
                                dynVideo.Title = src.Extend.OrigDesc.FirstOrDefault()?.Text;
                            }
                            dynItemModel.Video = dynVideo;
                            break;
                        }
                    case 512:
                        {
                            var seasonInfo = new DynamicSeasonCardApiSeasonInfoModel()
                            {
                                Title = src.Extend.OrigName,
                                SeasonId = moduleDynamic.ModuleDynamic.DynPgc.SeasonId,
                            };
                            var dynSeason = new DynamicSeasonCardModel()
                            {
                                Aid = moduleDynamic.ModuleDynamic.DynPgc.Aid.ToString(),
                                Cover = moduleDynamic.ModuleDynamic.DynPgc.Cover,
                                Season = seasonInfo,
                                NewDesc = src.Extend.OrigDesc.FirstOrDefault()?.Text,
                            };
                            dynItemModel.Season = dynSeason;
                            break;
                        }
                }

                dynamicItemModels.Add(dynItemModel);
            }

            return dynamicItemModels;
        }
    }
}

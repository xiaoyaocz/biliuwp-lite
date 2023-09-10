using System;
using System.Collections.Generic;
using AutoMapper;
using BiliLite.Controls.Dynamic;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Common.UserDynamic;
using BiliLite.Models.Dynamic;
using BiliLite.Models.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiliLite.Models.Builders
{
    // TODO: 后续改Factory模式返回不同类型的动态
    public class DynamicItemDisplayModelBuilder
    {
        private UserDynamicItemDisplayViewModel m_displayViewModel;
        private DynamicCardModel m_cardModel;

        public DynamicItemDisplayModelBuilder Init(IMapper mapper, DynamicCardModel item, JObject card)
        {
            this.m_cardModel = item;
            m_displayViewModel = mapper.Map<UserDynamicItemDisplayViewModel>(item.desc);
            if (m_displayViewModel.Type == UserDynamicDisplayType.Other)
            {
                var model = new UserDynamicItemDisplayViewModel
                {
                    Type = UserDynamicDisplayType.Other,
                    IntType = 999,
                    DynamicID = item.desc.dynamic_id,
                    ContentStr = $"未适配的类型{m_displayViewModel.IntType}:\r\n" + JsonConvert.SerializeObject(item)
                };

                throw new DynamicDisplayTypeUnsupportedException(model);
            }

            //var card = JObject.Parse(item.card);
            //var extendJson = JObject.Parse(item.extend_json);
            m_displayViewModel.OneRowInfo = DynamicParseExtensions.ParseOneRowInfo(m_displayViewModel.Type, card);

            return this;
        }

        public DynamicItemDisplayModelBuilder SetCommands(UserDynamicItemDisplayCommands commands)
        {
            m_displayViewModel.UserDynamicItemDisplayCommands = commands;
            return this;
        }

        public DynamicItemDisplayModelBuilder SwitchType(IMapper mapper, JObject card)
        {
            switch (m_displayViewModel.Type)
            {
                case UserDynamicDisplayType.ShortVideo:
                    m_displayViewModel.ShortVideoInfo = DynamicParseExtensions.ParseShortVideoInfo(card);
                    break;
                case UserDynamicDisplayType.Photo:
                    {
                        var imgs = new List<UserDynamicItemDisplayImageInfo>();
                        var allImageUrl = new List<string>();
                        var i = 0;
                        foreach (var img in card["item"]["pictures"])
                        {
                            allImageUrl.Add(img["img_src"].ToString());
                            var info = new UserDynamicItemDisplayImageInfo
                            {
                                ImageUrl = img["img_src"].ToString(),
                                Height = img["img_height"].ToInt32(),
                                Width = img["img_width"].ToInt32(),
                                Index = i,
                                ImageCommand = m_displayViewModel.UserDynamicItemDisplayCommands.ImageCommand
                            };
                            imgs.Add(info);
                            i++;
                        }

                        //偷懒方法，点击图片时可以获取全部图片信息，好孩子不要学
                        imgs.ForEach((x) => x.AllImages = allImageUrl);

                        m_displayViewModel.ImagesInfo = imgs;
                        break;
                    }
                case UserDynamicDisplayType.Repost when card.ContainsKey("origin_user"):
                    {
                        var originUser = JsonConvert.DeserializeObject<DynamicCardDescUserProfileModel>(card["origin_user"].ToString());
                        var model = new DynamicCardModel
                        {
                            extend_json = card["origin_extend_json"].ToString(),
                            card = card["origin"].ToString(),
                            display = m_cardModel.display?.origin,
                            desc = new DynamicCardDescModel()
                            {
                                user_profile = originUser,
                                uid = originUser.info.uid,
                                dynamic_id = m_cardModel.desc.orig_dy_id,
                                type = m_cardModel.desc.orig_type
                            }
                        };
                        var cardOrigin = JObject.Parse(card["origin"].ToString());
                        var extendJsonOrigin = JObject.Parse(card["origin_extend_json"].ToString());
                        UserDynamicItemDisplayViewModel originDisplayView;
                        try
                        {
                            originDisplayView = new DynamicItemDisplayModelBuilder()
                                .Init(mapper, model, cardOrigin)
                                .SetCommands(m_displayViewModel.UserDynamicItemDisplayCommands)
                                .SwitchType(mapper, cardOrigin)
                                .SetCommentCount(cardOrigin)
                                .SetContent(cardOrigin, extendJsonOrigin)
                                .SetSeasonInfo(cardOrigin)
                                .SetUserProfile().Build();
                        }
                        catch (DynamicDisplayTypeUnsupportedException ex)
                        {
                            originDisplayView = ex.ViewModel;
                        }

                        originDisplayView.IsRepost = true;
                        m_displayViewModel.OriginInfo = new List<UserDynamicItemDisplayViewModel>() { originDisplayView };
                        break;
                    }
                case UserDynamicDisplayType.Repost:
                    m_displayViewModel.OriginInfo = new List<UserDynamicItemDisplayViewModel>() {
                            new UserDynamicItemDisplayViewModel()
                            {
                                IsRepost=true,
                                IntType=1024,
                                Type= UserDynamicDisplayType.Miss
                            }
                        };
                    break;
            }

            return this;
        }

        public DynamicItemDisplayModelBuilder SetCommentCount(JObject card)
        {
            if (m_cardModel.desc.comment == 0 && card.ContainsKey("stat"))
            {
                m_displayViewModel.CommentCount = card["stat"]["reply"].ToInt32();
            }
            //Season数据会出现desc.comment为0的情况
            if (m_cardModel.desc.comment == 0 && card.ContainsKey("reply_count"))
            {
                m_displayViewModel.CommentCount = card["reply_count"].ToInt32();
            }
            //专栏数据会出现desc.comment为0的情况
            if (m_cardModel.desc.comment == 0 && card.ContainsKey("stats"))
            {
                m_displayViewModel.CommentCount = card["stats"]["reply"].ToInt32();
            }

            return this;
        }

        public DynamicItemDisplayModelBuilder SetContent(JObject card, JObject extendJson)
        {
            var content = "";
            //内容
            if (card.ContainsKey("item") && card["item"]["content"] != null)
            {
                content = card["item"]["content"]?.ToString();
                extendJson["at_control"] = card["item"]["ctrl"];
            }
            else if (card.ContainsKey("item") && card["item"]["description"] != null)
            {
                content = card["item"]["description"]?.ToString();
                extendJson["at_control"] = card["item"]["at_control"];
            }
            else if (card.ContainsKey("dynamic"))
            {
                content = card["dynamic"]?.ToString();
            }
            else if (card.ContainsKey("vest") && card["vest"]["content"] != null)
            {
                content = card["vest"]["content"]?.ToString();
            }

            if (!string.IsNullOrEmpty(content))
            {
                m_displayViewModel.ContentStr = content;
                m_displayViewModel.Content = content.UserDynamicStringToRichText(m_cardModel.desc.dynamic_id, m_cardModel.display?.emoji_info?.emoji_details, extendJson);
            }
            else
            {
                m_displayViewModel.ShowContent = false;
            }

            return this;
        }

        public DynamicItemDisplayModelBuilder SetSeasonInfo(JObject card)
        {
            if (card.ContainsKey("apiSeasonInfo"))
            {
                m_displayViewModel.UserName = card["apiSeasonInfo"]["title"].ToString();
                m_displayViewModel.Photo = card["apiSeasonInfo"]["cover"].ToString();
                m_displayViewModel.TagName = card["apiSeasonInfo"]["type_name"].ToString();
                m_displayViewModel.ShowTag = true;
                m_displayViewModel.Time = m_displayViewModel.Time + "更新了";
            }
            if (card.ContainsKey("season"))
            {
                m_displayViewModel.UserName = card["season"]["title"].ToString();
                m_displayViewModel.Photo = card["season"]["cover"].ToString();
                m_displayViewModel.TagName = card["season"]["type_name"].ToString();
                m_displayViewModel.ShowTag = true;
                m_displayViewModel.Time = m_displayViewModel.Time + "更新了";
            }

            return this;
        }

        public DynamicItemDisplayModelBuilder SetUserProfile()
        {
            if (m_cardModel.desc.user_profile == null) return this;
            m_displayViewModel.UserName = m_cardModel.desc.user_profile.info.uname;
            m_displayViewModel.Photo = m_cardModel.desc.user_profile.info.face;
            if (m_cardModel.desc.user_profile.vip != null)
            {
                m_displayViewModel.IsYearVip = m_cardModel.desc.user_profile.vip.vipStatus == 1 && m_cardModel.desc.user_profile.vip.vipType == 2;
            }
            switch (m_cardModel.desc.user_profile.card?.official_verify?.type ?? 3)
            {
                case 0:
                    m_displayViewModel.Verify = Constants.App.VERIFY_PERSONAL_IMAGE;
                    break;
                case 1:
                    m_displayViewModel.Verify = Constants.App.VERIFY_OGANIZATION_IMAGE;
                    break;
                default:
                    m_displayViewModel.Verify = Constants.App.TRANSPARENT_IMAGE;
                    break;
            }
            if (!string.IsNullOrEmpty(m_cardModel.desc.user_profile.pendant?.image))
            {
                m_displayViewModel.Pendant = m_cardModel.desc.user_profile.pendant.image;
            }
            //装扮
            m_displayViewModel.DecorateName = m_cardModel.desc.user_profile.decorate_card?.name;
            m_displayViewModel.DecorateText = m_cardModel.desc.user_profile.decorate_card?.fan?.num_desc;
            m_displayViewModel.DecorateColor = m_cardModel.desc.user_profile.decorate_card?.fan?.color;
            m_displayViewModel.DecorateImage = m_cardModel.desc.user_profile.decorate_card?.big_card_url;

            return this;
        }

        public UserDynamicItemDisplayViewModel Build()
        {
            return m_displayViewModel;
        }
    }
}

using System.Windows.Input;

namespace BiliLite.Models.Common.UserDynamic
{
    public class UserDynamicItemDisplayCommands
    {        
        /// <summary>
        /// 打开用户个人中心
        /// </summary>
        public ICommand UserCommand { get; set; }

        /// <summary>
        /// 打开抽奖信息
        /// </summary>
        public ICommand LotteryCommand { get; set; }

        /// <summary>
        /// 打开链接
        /// </summary>
        public ICommand LaunchUrlCommand { get; set; }

        /// <summary>
        /// 打开网页
        /// </summary>
        public ICommand WebCommand { get; set; }

        /// <summary>
        /// 打开话题
        /// </summary>
        public ICommand TagCommand { get; set; }

        /// <summary>
        /// 打开投票信息
        /// </summary>
        public ICommand VoteCommand { get; set; }

        /// <summary>
        /// 查看图片信息
        /// </summary>
        public ICommand ImageCommand { get; set; }

        /// <summary>
        /// 点赞
        /// </summary>
        public ICommand LikeCommand { get; set; }

        /// <summary>
        /// 评论
        /// </summary>
        public ICommand CommentCommand { get; set; }

        /// <summary>
        /// 转发
        /// </summary>
        public ICommand RepostCommand { get; set; }

        /// <summary>
        /// 删除动态
        /// </summary>
        public ICommand DeleteCommand { get; set; }

        /// <summary>
        /// 打开详情页面
        /// </summary>
        public ICommand DetailCommand { get; set; }

        /// <summary>
        /// 添加到稍后再看
        /// </summary>
        public ICommand WatchLaterCommand { get; set; }
    }
}
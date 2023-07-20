using BiliLite.ViewModels.Comment;
using BiliLite.ViewModels.Download;
using BiliLite.ViewModels.User;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Extensions
{
    public static class ViewModelExtensions
    {
        public static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            services.AddTransient<DownloadDialogViewModel>();
            services.AddTransient<CommentControlViewModel>();
            services.AddTransient<UserSubmitVideoViewModel>();
            return services;
        }
    }
}

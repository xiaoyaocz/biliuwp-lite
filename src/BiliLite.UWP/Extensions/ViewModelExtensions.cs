using BiliLite.ViewModels.Comment;
using BiliLite.ViewModels.Download;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Extensions
{
    public static class ViewModelExtensions
    {
        public static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            services.AddTransient<DownloadDialogViewModel>();
            services.AddTransient<CommentControlViewModel>();
            return services;
        }
    }
}

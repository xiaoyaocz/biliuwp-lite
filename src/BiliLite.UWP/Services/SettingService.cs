using BiliLite.Models;
using BiliLite.Models.Common;
using Microsoft.Toolkit.Uwp.Helpers;

namespace BiliLite.Services
{
    public class SettingService
    {
        private static LocalObjectStorageHelper storageHelper = new LocalObjectStorageHelper();

        public static T GetValue<T>(string key, T _default)
        {
            if (storageHelper.KeyExists(key))
            {
                return storageHelper.Read<T>(key);
            }
            else
            {
                return _default;
            }
        }

        public static void SetValue<T>(string key, T value)
        {
            storageHelper.Save<T>(key, value);
        }

        public class UI
        {
            private static bool? _loadOriginalImage = null;

            public static bool? LoadOriginalImage
            {
                get
                {
                    if (_loadOriginalImage == null)
                    {
                        _loadOriginalImage = GetValue(SettingConstants.UI.ORTGINAL_IMAGE, false);
                    }
                    return _loadOriginalImage.Value;
                }
                set => _loadOriginalImage = value;
            }
        }

        public class Account
        {
            public static MyProfileModel Profile
            {
                get
                {
                    return storageHelper.Read<MyProfileModel>(SettingConstants.Account.USER_PROFILE);
                }
            }
            public static bool Logined
            {
                get
                {
                    return storageHelper.KeyExists(SettingConstants.Account.ACCESS_KEY) && !string.IsNullOrEmpty(storageHelper.Read<string>(SettingConstants.Account.ACCESS_KEY, null));
                }
            }
            public static string AccessKey
            {
                get
                {
                    return GetValue(SettingConstants.Account.ACCESS_KEY, "");
                }
            }
            public static int UserID
            {
                get
                {
                    return GetValue(SettingConstants.Account.USER_ID, 0);
                }
            }
        }
    }
}

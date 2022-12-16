namespace Virterix.AdMediation
{
    public class YandexAdInstanceBannerParameters : AdInstanceParameters
    {
        public const string _AD_INSTANCE_PARAMETERS_FOLDER = "YandexMobileAds";
        public const string _PARAMETERS_FILE_NAME = "YandexMobileAds_AdInstanceBanner";

        public YandexMobileAdsAdapter.YandexBannerSize m_bannerSize;
        
        public override AdType AdvertiseType => AdType.Banner;
        public bool m_prepareOnStart;
        public int m_maxHeight;
        public int m_refreshTime;
        
#if UNITY_EDITOR
        public static AdInstanceParameters CreateParameters()
        {
            var parameters = CreateParameters<YandexAdInstanceBannerParameters>(_AD_INSTANCE_PARAMETERS_FOLDER, _PARAMETERS_FILE_NAME);
            return parameters;
        }

        public static YandexAdInstanceBannerParameters CreateParameters(string projectName, string postfixName)
        {
            var parameters = CreateParameters<YandexAdInstanceBannerParameters>(projectName, _AD_INSTANCE_PARAMETERS_FOLDER, 
                _PARAMETERS_FILE_NAME + postfixName);
            return parameters;
        }
#endif
    }
}
namespace Virterix.AdMediation
{
    public class AppodealAdInstanceBannerParameters : AdInstanceParameters
    {
        public const string _AD_INSTANCE_PARAMETERS_FOLDER = "Appodeal";
        public const string _PARAMETERS_FILE_NAME = "Appodeal_AdInstanceBanner";
        
        public AppodealAdapter.AppodealBannerSize m_bannerSize;
        
        public override AdType AdvertiseType => AdType.Banner;

#if UNITY_EDITOR
        public static AdInstanceParameters CreateParameters()
        {
            var parameters =
                CreateParameters<AppodealAdInstanceBannerParameters>(
                    _AD_INSTANCE_PARAMETERS_FOLDER,
                    _PARAMETERS_FILE_NAME);
            return parameters;
        }

        public static AppodealAdInstanceBannerParameters CreateParameters(string projectName, string postfixName)
        {
            var parameters = CreateParameters<AppodealAdInstanceBannerParameters>(projectName,
                _AD_INSTANCE_PARAMETERS_FOLDER,
                _PARAMETERS_FILE_NAME + postfixName);
            return parameters;
        }
#endif
    }
}
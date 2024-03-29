namespace Virterix.AdMediation
{
    public class IronSourceAdInstanceBannerParameters : AdInstanceParameters
    {
        public const string _AD_INSTANCE_PARAMETERS_FOLDER = "IronSource";
        public const string _PARAMETERS_FILE_NAME = "IronSource_AdInstanceBanner";

        public IronSourceAdapter.IrnSrcBannerSize m_bannerSize;

        public override AdType AdvertiseType => AdType.Banner;

#if UNITY_EDITOR
        public static AdInstanceParameters CreateParameters()
        {
            var parameters =
                CreateParameters<IronSourceAdInstanceBannerParameters>(_AD_INSTANCE_PARAMETERS_FOLDER,
                    _PARAMETERS_FILE_NAME);
            return parameters;
        }

        public static IronSourceAdInstanceBannerParameters CreateParameters(string projectName, string postfixName)
        {
            var parameters = CreateParameters<IronSourceAdInstanceBannerParameters>(projectName,
                _AD_INSTANCE_PARAMETERS_FOLDER,
                _PARAMETERS_FILE_NAME + postfixName);
            return parameters;
        }
#endif
    }
}
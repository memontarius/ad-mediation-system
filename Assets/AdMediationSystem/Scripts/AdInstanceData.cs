using UnityEngine;

namespace Virterix.AdMediation
{
    public class AdInstanceData
    {
        public const string _AD_INSTANCE_DEFAULT_NAME = "Default";

        public AdInstanceData()
        {
        }

        public AdInstanceData(AdType adType, string adID, string name = _AD_INSTANCE_DEFAULT_NAME)
        {
            m_enabledState = true;
            m_adType = adType;
            m_adID = adID;
            Name = name;
            m_isDefault = Name == _AD_INSTANCE_DEFAULT_NAME;
        }

        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
                m_isDefault = m_name == _AD_INSTANCE_DEFAULT_NAME;
            }
        }
        string m_name;

        public string ParametersName
        {
            get { return m_adInstanceParams != null ? m_adInstanceParams.Name : AdInstanceParameters._AD_INSTANCE_PARAMETERS_DEFAULT_NAME; }
        }

        public bool IsDefault
        {
            get { return m_isDefault; }
        }
        bool m_isDefault = true;

        public AdType m_adType;
        public string m_adID;
        public AdNetworkAdapter.TimeoutParams? m_timeout;
        public bool m_isBannerAdTypeVisibled;
        //public Vector2 m_bannerCoordinates;
        public AdNetworkAdapter.AdState m_state = AdNetworkAdapter.AdState.Uncertain;
        public bool m_lastAdPrepared;
        public bool m_enabledState;
        public object m_adView;
        public IAdInstanceParameters m_adInstanceParams;
        public float m_startImpressionTime;
        public float m_displayTime;
    }
} // namespace Virterix.AdMediation

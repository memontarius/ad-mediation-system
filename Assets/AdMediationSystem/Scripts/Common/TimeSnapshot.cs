using UnityEngine;
using System;
using System.Globalization;

namespace Virterix.Common
{
    public class TimeSnapshot
    {
        const string _DEFAULT_ENCRYPTION_KEY = "yxmwi";

        public enum PeriodType
        {
            Seconds,
            Minutes,
            Hours,
            Days
        }
        
        string m_key;
        public float m_period;
        public PeriodType m_periodType;

        DateTime m_savedDate;
        bool m_wasSaved;
        bool m_useEncryption;
        string m_encryptionKey = "";

        string EncryptionKey
        {
            get
            {
                return m_encryptionKey.Length != 0 ? m_encryptionKey : _DEFAULT_ENCRYPTION_KEY;
            }
        }

        public bool WasSaved
        {
            get { return m_wasSaved; }
        }

        public TimeSpan PassedTimeSpanSinceLastSave
        {
            get
            {
                TimeSpan elapsedTimeSpan;
                if (WasSaved)
                {
                    elapsedTimeSpan = DateTime.Now.ToUniversalTime().Subtract(m_savedDate);
                }
                else
                {
                    elapsedTimeSpan = new TimeSpan();
                }
                return elapsedTimeSpan;
            }
        }

        public double PassedSecondsSinceLastSave
        {
            get { return PassedTimeSpanSinceLastSave.TotalSeconds; }
        }

        public double PassedMinutesSinceLastSave
        {
            get { return PassedTimeSpanSinceLastSave.TotalMinutes; }
        }

        public double PassedHoursSinceLastSave
        {
            get { return PassedTimeSpanSinceLastSave.TotalHours; }
        }

        public double PassedDaysSinceLastSave
        {
            get { return PassedTimeSpanSinceLastSave.TotalDays; }
        }

        public bool IsPeriodOver
        {
            get
            {
                bool isOver = false;

                if (m_wasSaved)
                {
                    double elapsedPeriod = 0;
                    TimeSpan elapsedTimeSpan = PassedTimeSpanSinceLastSave;

                    switch (m_periodType)
                    {
                        case PeriodType.Seconds:
                            elapsedPeriod = elapsedTimeSpan.TotalSeconds;
                            break;
                        case PeriodType.Minutes:
                            elapsedPeriod = elapsedTimeSpan.TotalMinutes;
                            break;
                        case PeriodType.Hours:
                            elapsedPeriod = elapsedTimeSpan.TotalHours;
                            break;
                        case PeriodType.Days:
                            elapsedPeriod = elapsedTimeSpan.TotalDays;
                            break;
                    }

                    if (elapsedPeriod > m_period)
                    {
                        isOver = true;
                    }
                }

                return isOver;
            }
        }

        public TimeSnapshot(string key, float period, PeriodType periodType = PeriodType.Hours, bool useEncryption = false, string encryptionKey = "")
        {
            m_key = key;
            m_period = period;
            m_periodType = periodType;
            m_useEncryption = useEncryption;
            m_encryptionKey = encryptionKey;
            m_wasSaved = GetSavedDateTime(ref m_savedDate);
        }

        bool GetSavedDateTime(ref DateTime date)
        {
            bool wasSave = false;
            string stringDateTime = PlayerPrefs.GetString(m_key, "");

            if (stringDateTime.Length != 0)
            {
                if (m_useEncryption)
                    stringDateTime = CryptString.Decode(stringDateTime, EncryptionKey);

                wasSave = DateTime.TryParse(stringDateTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
                if (!wasSave)
                    date = DateTime.UtcNow;
            }
            return wasSave;
        }

        public void Save(bool isUpdateCurrSavedData = true)
        {
            DateTime currDateTime = DateTime.UtcNow;
            if (isUpdateCurrSavedData)
                m_savedDate = currDateTime;
            
            string currDateTimeStr = currDateTime.ToString(CultureInfo.InvariantCulture);
            if (m_useEncryption)
                currDateTimeStr = CryptString.Encode(currDateTimeStr, EncryptionKey);
            
            PlayerPrefs.SetString(m_key, currDateTimeStr);
        }
    }
}
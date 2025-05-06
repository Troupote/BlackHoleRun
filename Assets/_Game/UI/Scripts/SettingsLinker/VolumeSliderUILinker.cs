using UnityEngine;
using UnityEngine.UI;

namespace BHR.UILinkers
{
    public class VolumeSliderUILinker : ASettingsUILinker<float>
    {
        public enum VolumeType { MASTER, MUSIC, SOUNDS}
        [SerializeField]
        private VolumeType volumeType;
        public override void SaveSetting(float value)
        {
            switch(volumeType)
            {
                case VolumeType.MASTER: SettingsSave.SaveMasterVolume(value); break;
                case VolumeType.MUSIC: SettingsSave.SaveMusicVolume(value); break;
                case VolumeType.SOUNDS: SettingsSave.SaveSoundsVolume(value); break;
            }
            SettingsManager.Instance.UpdateVolume();
        }

        protected override float LoadSetting()
        {
            float value = 0f;
            switch(volumeType)
            {
                case VolumeType.MASTER: value = SettingsSave.LoadMasterVolume(); break;
                case VolumeType.MUSIC: value = SettingsSave.LoadMusicVolume(); break;
                case VolumeType.SOUNDS: value = SettingsSave.LoadSoundsVolume(); break;
            }

            return value;
        }

        protected override void UpdateUI() => GetComponent<Slider>().SetValueWithoutNotify(LoadSetting());
    }
}

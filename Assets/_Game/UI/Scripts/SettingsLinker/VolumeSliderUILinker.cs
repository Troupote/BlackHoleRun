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

        protected override void LoadSetting()
        {
            Slider slider = GetComponent<Slider>();
            switch(volumeType)
            {
                case VolumeType.MASTER: slider.value = SettingsSave.LoadMasterVolume(); break;
                case VolumeType.MUSIC: slider.value = SettingsSave.LoadMusicVolume(); break;
                case VolumeType.SOUNDS: slider.value = SettingsSave.LoadSoundsVolume(); break;
            }
        }
    }
}

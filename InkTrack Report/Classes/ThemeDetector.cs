using Microsoft.Win32;

namespace InkTrack_Report.Classes
{
    public enum AppTheme
    {
        Light,
        Dark
    }

    public static class ThemeDetector
    {
        public static AppTheme GetWindowsTheme()
        {
            const string registryKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKey))
            {
                if (key != null)
                {
                    object registryValueObject = key.GetValue("SystemUsesLightTheme");
                    if (registryValueObject != null)
                    {
                        int registryValue = (int)registryValueObject;
                        return registryValue > 0 ? AppTheme.Light : AppTheme.Dark;
                    }
                }
            }

            // По умолчанию, если не удалось определить — возвращаем светлую
            return AppTheme.Light;
        }
    }

}

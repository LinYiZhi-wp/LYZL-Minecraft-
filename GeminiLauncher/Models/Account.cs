using CommunityToolkit.Mvvm.ComponentModel;

namespace GeminiLauncher.Models
{
    public enum AccountType
    {
        Offline,
        Microsoft
    }

    public partial class Account : ObservableObject
    {
        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _uuid = string.Empty;

        [ObservableProperty]
        private string _accessToken = string.Empty;
        
        // Microsoft Auth specific
        [ObservableProperty]
        private string _refreshToken = string.Empty;

        [ObservableProperty]
        private string _minecraftAccessToken = string.Empty;

        [ObservableProperty]
        private long _expiryTime = 0;
        
        [ObservableProperty]
        private AccountType _type = AccountType.Offline;

        [ObservableProperty]
        private string _avatarUrl = string.Empty;
    }
}

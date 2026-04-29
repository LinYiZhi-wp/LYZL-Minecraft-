using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using GeminiLauncher.Models;

namespace GeminiLauncher.Services
{
    public partial class AccountManager : ObservableObject
    {
        private readonly AuthenticationService _authService;

        [ObservableProperty]
        private Account? _currentAccount;

        public ObservableCollection<Account> Accounts { get; } = new ObservableCollection<Account>();

        public AccountManager()
        {
            _authService = new AuthenticationService();
        }

        public void AddAccount(Account account)
        {
            var existing = Accounts.FirstOrDefault(a => a.Uuid == account.Uuid);
            if (existing != null)
            {
                Accounts.Remove(existing);
            }
            Accounts.Add(account);
            CurrentAccount = account;
        }

        public void LoginOffline(string username)
        {
            var account = _authService.LoginOffline(username);
            AddAccount(account);
        }

        public async System.Threading.Tasks.Task LoginMicrosoft()
        {
            var account = await _authService.LoginMicrosoftAsync();
            AddAccount(account);
        }

        public Account? ActiveAccount => CurrentAccount;

        public void Logout()
        {
            if (CurrentAccount != null)
            {
                RemoveAccount(CurrentAccount);
            }
        }

        [CommunityToolkit.Mvvm.Input.RelayCommand]
        public void RemoveAccount(Account account)
        {
             if (Accounts.Contains(account))
            {
                Accounts.Remove(account);
                if (CurrentAccount == account)
                {
                    CurrentAccount = Accounts.FirstOrDefault();
                }
            }
        }
    }
}

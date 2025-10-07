using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace if_hub.Auth
{
    public class CustomAuthenticationStateProvider : ServerAuthenticationStateProvider, IDisposable
    {
        private readonly IServiceScope _scope;
        private readonly AuthenticationStateProvider _baseProvider;

        public CustomAuthenticationStateProvider(IServiceProvider serviceProvider)
        {
            _scope = serviceProvider.CreateScope();
            _baseProvider = _scope.ServiceProvider.GetRequiredService<ServerAuthenticationStateProvider>();
            _baseProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return await _baseProvider.GetAuthenticationStateAsync();
        }

        private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            NotifyAuthenticationStateChanged(task);
        }
        
        public void Dispose()
        {
            _baseProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
            _scope.Dispose();
        }
    }
}
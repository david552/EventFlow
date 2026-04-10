using EventFlow.Application.GlobalSettings.Responses;

namespace EventFlow.MVC.Models
{
    public class SettingsViewModel
    {
        public List<GlobalSettingsResponseModel> Settings { get; set; } = new();
    }
}
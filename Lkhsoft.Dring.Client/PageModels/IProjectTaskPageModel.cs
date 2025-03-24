#region

using CommunityToolkit.Mvvm.Input;
using Lkhsoft.Dring.Client.Models;

#endregion

namespace Lkhsoft.Dring.Client.PageModels;

public interface IProjectTaskPageModel
{
    IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
    bool IsBusy { get; }
}
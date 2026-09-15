using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace WebClient.Features.Actors.Search.Components
{
    public partial class ActorResultsTable
    {
        [Parameter, EditorRequired] public SearchActors.Response? Response {get;set;}
        [Parameter, EditorRequired] public EventCallback<int> PageNumberChanged {get;set;}
        [Parameter, EditorRequired] public EventCallback<int> PageSizeChanged {get;set;}
        [Parameter, EditorRequired] public EventCallback<SearchActors.ActorSummary> ActorSelected {get;set;}
        [Parameter, EditorRequired] public bool SortByDescending {get;set;}
        [Parameter, EditorRequired] public EventCallback<bool> SortByDescendingChanged {get;set;}

        private Task OnRowClicked(TableRowClickEventArgs<SearchActors.ActorSummary> args) => ActorSelected.InvokeAsync(args.Item);
    }
}

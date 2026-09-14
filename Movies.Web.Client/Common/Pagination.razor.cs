using Microsoft.AspNetCore.Components;

namespace WebClient.Common
{
    public partial class Pagination
    {
        private static readonly int[] PageSizeOptions = [10, 20, 50, 100];

        [Parameter, EditorRequired] public int PageNumber {get;set;}
        [Parameter, EditorRequired] public int PageSize {get;set;}
        [Parameter, EditorRequired] public int TotalPages {get;set;}
        [Parameter, EditorRequired] public EventCallback<int> PageNumberChanged {get;set;}
        [Parameter, EditorRequired] public EventCallback<int> PageSizeChanged {get;set;}

        private Task OnPageNumberChanged(int page) => PageNumberChanged.InvokeAsync(page);
        private Task OnPageSizeChanged(int size) => PageSizeChanged.InvokeAsync(size);
    }
}

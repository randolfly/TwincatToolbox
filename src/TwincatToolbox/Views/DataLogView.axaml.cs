using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;

using System.Collections.Generic;
using System.Linq;
using System.Threading;

using TwincatToolbox.Models;
using TwincatToolbox.ViewModels;

namespace TwincatToolbox.Views;

public partial class DataLogView : UserControl
{
    private Timer _searchTimer;

    public DataLogView() {
        InitializeComponent();
        _searchTimer = new Timer(OnSearchTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
    }

    private void TextBox_TextChanged(object? sender, TextChangedEventArgs e) {
        _searchTimer.Change(200, Timeout.Infinite); // 200ms 延迟
    }

    private void OnSearchTimerElapsed(object state) {
        // 在 UI 线程上执行搜索命令
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext is DataLogViewModel viewModel)
            {
                var searchResultsVisibility = viewModel.SearchSymbols();
                // 更新搜索结果（隐藏搜索出的ListViewItem）[IsVisible=true]
                UpdateListViewVisibility(searchResultsVisibility);
            }
        });
    }

    private void UpdateListViewVisibility(IEnumerable<bool> searchResultsVisibility) {
        var listView = this.FindControl<ListBox>("AvailableSymbolListView");
        if (listView != null)
        {
            var listViewItems = listView.GetLogicalChildren().OfType<ListBoxItem>().ToList();
            for (var i = 0; i < listViewItems.Count; i++)
            {
                listViewItems[i].IsVisible = searchResultsVisibility.ElementAt(i);
            }
        }
    }
}

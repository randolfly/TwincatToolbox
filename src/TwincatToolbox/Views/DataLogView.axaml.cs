using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;

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
        _searchTimer.Change(1000, Timeout.Infinite); // 200ms 延迟
    }

    private void OnSearchTimerElapsed(object state) {
        // 在 UI 线程上执行搜索命令
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext is DataLogViewModel viewModel)
            {
                var searchResults = viewModel.SearchSymbol();
                Debug.WriteLine("Search results: {0}", searchResults.Count());
                // 更新搜索结果（隐藏搜索出的TreeViewItem）[IsVisible=true]
                UpdateTreeViewVisibility(searchResults);
                Debug.WriteLine("Update UI Finished");
            }
        });
    }

    private void UpdateTreeViewVisibility(IEnumerable<SymbolNode> searchResults) {
        var treeView = this.FindControl<TreeView>("AvailableSymbolTreeView");
        if (treeView != null)
        {
            foreach(var searchSymbol in searchResults)
            {
                var viewItem = FindTreeViewItem(treeView, searchSymbol);
                if (viewItem != null)
                {
                    viewItem.IsVisible = false;
                }
            }
        }
    }

    private TreeViewItem? FindTreeViewItem(ItemsControl parent, SymbolNode node) {
        foreach(var child in parent.GetVisualChildren())
        {
            if (child is TreeViewItem treeViewItem)
            {
                var result = FindTreeViewItem(treeViewItem, node);
                if (result != null)
                {
                    return result;
                }
            }
        }
        return null;
    }
}

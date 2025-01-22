using System;
using System.ComponentModel;
using System.Linq;

using Avalonia.Markup.Xaml.MarkupExtensions;

using CommunityToolkit.Mvvm.ComponentModel;

using TwinCAT.TypeSystem;

namespace TwincatToolbox.Models;

// todo: 检查这里的 partial class是否有必要
public class SymbolInfo(ISymbol symbol)
{
    public ISymbol Symbol { get; set; } = symbol;
    public string Name => string.Join(".",
        Symbol.InstancePath.Split('.').Skip(1));
}
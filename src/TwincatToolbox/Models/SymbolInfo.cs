using System;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;

using TwinCAT.TypeSystem;

namespace TwincatToolbox.Models;

public partial class SymbolInfo(ISymbol symbol)
{
    public ISymbol Symbol { get; set; } = symbol;
    public string Name => string.Join(".",
        Symbol.InstancePath.Split('.').Skip(1));
}
using System;
using System.Linq;
using TwinCAT.TypeSystem;

namespace TwincatToolbox.Models;

public class SymbolInfo(ISymbol symbol)
{
    public ISymbol Symbol { get; set; } = symbol;
    public string Name => string.Join(".",
        Symbol.InstancePath.Split('.').Skip(1));
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwincatToolbox.Models;

namespace TwincatToolbox.Extensions;
public static class SymbolInfoExtension
{
    public static SymbolInfo DeepCopy(this SymbolInfo symbol)
    {
        return new SymbolInfo(symbol.Symbol);
    }
}
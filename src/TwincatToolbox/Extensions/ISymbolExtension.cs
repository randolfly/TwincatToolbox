using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwinCAT.TypeSystem;

namespace TwincatToolbox.Extensions;
public static class ISymbolExtension
{
    public static string GetSymbolName(this ISymbol symbol) {
        return symbol.InstancePath.ToLowerInvariant();
    }
}

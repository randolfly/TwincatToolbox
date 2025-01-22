using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwinCAT.TypeSystem;

namespace TwincatToolbox.Extensions;
public static class ISymbolExtension
{
    // note: this method can cause problems if the symbol name is not unique
    public static string GetSymbolName(this ISymbol symbol) {
        return symbol.InstanceName.ToLower();
    }
}

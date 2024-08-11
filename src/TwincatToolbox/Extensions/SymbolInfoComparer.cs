using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwincatToolbox.Models;

namespace TwincatToolbox.Extensions;
public class SymbolInfoComparer : IEqualityComparer<SymbolInfo>, IComparer<SymbolInfo>
{
    public bool Equals(SymbolInfo? x, SymbolInfo? y) {
        if (x == null || y == null)
            return false;

        // 自定义比较逻辑，忽略大小写
        return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(SymbolInfo? obj) {
        if (obj == null)
            return 0;

        // 使用 Name 的哈希码，忽略大小写
        return obj.Name?.ToLower().GetHashCode() ?? 0;
    }

    public int Compare(SymbolInfo? x, SymbolInfo? y) => x.Name.CompareTo(y.Name);

    public static SymbolInfoComparer Instance { get; } = new SymbolInfoComparer();
}
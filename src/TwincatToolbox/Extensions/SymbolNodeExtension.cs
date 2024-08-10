using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwinCAT.TypeSystem;

using TwincatToolbox.Models;

namespace TwincatToolbox.Extensions;
public static class SymbolNodeExtension
{
    public static void ToIEnumerable(this SymbolNode node, ref List<SymbolNode> result) 
    {
        result.Add(node);
        if (node.SubSymbolNodes != null)
        {
            foreach (var subNode in node.SubSymbolNodes)
            {
                subNode.ToIEnumerable(ref result);
            }
        }
    }
}
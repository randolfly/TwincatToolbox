using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;

namespace TwincatToolbox.Models;
public class SymbolNode
{
    public ObservableCollection<SymbolNode>? SubSymbolNodes { get; set; }
    public string Name => Symbol.InstancePath;
    public ISymbol Symbol { get; set; }
    public SymbolNode(ISymbol symbol)
    {
        Symbol = symbol;
    }
    public SymbolNode(ISymbol symbol, ObservableCollection<SymbolNode> subNodes) {
        Symbol = symbol;
        SubSymbolNodes = subNodes;
    }
}
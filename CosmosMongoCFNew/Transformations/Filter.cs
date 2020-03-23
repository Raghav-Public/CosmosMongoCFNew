using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosMongoCFNew.Transformations
{
    public abstract class Filter
    {
        public abstract string Type { get; }
        public abstract void Execute(Object obj);
    }
}

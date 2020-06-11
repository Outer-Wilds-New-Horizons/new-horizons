using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marshmallow.Utility
{
    public class Tuple
    {
        public Tuple(params object[] _items)
        {
            Items = _items.ToList();
        }

        public List<object> Items { get; }
    }
}

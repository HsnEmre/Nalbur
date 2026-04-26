using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nalbur.Infrastructure.Services
{
    public class ExportColumn<T>
    {
        public string Header { get; }
        public Func<T, object?> ValueSelector { get; }

        public ExportColumn(string header, Func<T, object?> valueSelector)
        {
            Header = header;
            ValueSelector = valueSelector;
        }
    }
}

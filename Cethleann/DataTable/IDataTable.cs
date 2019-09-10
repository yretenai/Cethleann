using System;
using System.Collections.Generic;
using System.Text;

namespace Cethleann.DataTable
{
    public interface IDataTable
    {
        public List<byte[]> Data { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskList.Models
{
    /// <summary>
    /// ViewModel for the Storage Token response - used for serialization
    /// to JSON.
    /// </summary>
    public class StorageTokenViewModel
    {
        public string Name { get; set; }
        public Uri Uri { get; set; }
        public string SasToken { get; set; }
    }
}

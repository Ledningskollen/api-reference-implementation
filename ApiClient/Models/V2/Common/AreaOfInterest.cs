using System.Collections.Generic;
using Api.Interfaces.V2.Common;

namespace ApiClient.Models.V2.Common {
    /// <summary>
    /// This model represents area of interest information
    /// </summary>
    public class AreaOfInterest: IAreaOfInterest {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Geometry { get; set; }
        public Dictionary<string, string> Recipients { get; set; }
    }
}
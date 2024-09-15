using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionFlowerAPI.DTO
{
    public class PlantIdentificationResult
    {
        public string ScientificName { get; set; }
        public List<ImageInfo> Images { get; set; }
    }
}

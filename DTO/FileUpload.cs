using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionFlowerAPI.DTO
{
    public class FlowerRecognitionRequest
    {
        [OpenApiProperty(Description = "Image file for flower recognition")]
        public byte[] File { get; set; }
    }

    // Clase para representar el resultado del reconocimiento de flores
    public class RecognitionResult
    {
        public string FlowerName { get; set; }
        public double Confidence { get; set; }
    }
}

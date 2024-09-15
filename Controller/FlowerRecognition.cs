using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using AzureFunctionFlowerAPI.DTO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AzureFunctionFlowerAPI.Controller
{
    public static class FlowerRecognition
    {

        private static readonly HttpClient client = new HttpClient();

        [FunctionName("FlowerRecognition")]
        [OpenApiOperation(operationId: "FlowerRecognition", tags: new[] { "FlowerRecognition" })]
        [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(FlowerRecognitionRequest), Required = true, Description = "Upload an image for flower recognition")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(OkObjectResult), Description = "Plant Recognition")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Description = "Error 500 Internal Server Error")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(string), Description = "Error 400 Bad Request")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Iniciando proceso de identificacion de imagen.");

            // Definicion de constantes del api
            const string project = "all";
            const bool includeRelatedImages = true;
            const bool noReject = false;
            const int nbResults = 2;
            const string lang = "es";
            const string apiKey = "api-key";

            // Validacion de imagen en el body
            var formFiles = req.Form.Files;
            if (formFiles.Count == 0)
            {
                return new BadRequestObjectResult("No se ha enviado una imagen");
            }

            // Crear el request multipart/form-data para la API de identificación
            var requestContent = new MultipartFormDataContent();
            foreach (var formFile in formFiles)
            {
                var stream = formFile.OpenReadStream();
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                requestContent.Add(streamContent, "images", formFile.FileName);
            }


            // Construir la URL del endpoint con valores constantes
            string apiUrl = $"https://my-api.plantnet.org/v2/identify/{project}?include-related-images={includeRelatedImages}&no-reject={noReject}&nb-results={nbResults}&lang={lang}&api-key={apiKey}";

            // Realizar la solicitud POST
            HttpResponseMessage response = await client.PostAsync(apiUrl, requestContent);

            // Procesar la respuesta
            string responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                log.LogError($"Error calling API: {responseContent}");
                return new StatusCodeResult((int)response.StatusCode);
            }

            dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);

            // Mapear la información importante a la clase
            var bestMatch = jsonResponse.results[0];  // Asumiendo que el primer resultado es el mejor
            var plantResult = new PlantIdentificationResult
            {
                ScientificName = bestMatch.species.scientificName.ToString(),
                Images = new List<ImageInfo>()
            };

            foreach (var image in bestMatch.images)
            {
                plantResult.Images.Add(new ImageInfo
                {
                    Organ = image.organ.ToString(),
                    Author = image.author.ToString(),
                    License = image.license.ToString(),
                    Url = image.url.o.ToString()  
                });
            }

            // Devolver la información mapeada
            return new OkObjectResult(plantResult);

        }
    }


}

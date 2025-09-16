using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using ocpa.ro.api.Exceptions;
using ocpa.ro.common;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ocpa.ro.api.Helpers.Generic
{
    public interface IMultipartRequestHelper
    {
        Task<byte[]> GetMultipartRequestData(HttpRequest request);
    }

    public class MultipartRequestHelper : IMultipartRequestHelper
    {
        const int MultipartBoundaryLengthLimit = 8192;

        public async Task<byte[]> GetMultipartRequestData(HttpRequest request)
        {
            if (!(request?.ContentType?.Length > 0))
                throw new ExtendedException("No content type");

            if (!request.ContentType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase))
                throw new ExtendedException("Content type is not multipart");

            var boundary = GetBoundary(MediaTypeHeaderValue.Parse(request.ContentType), MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, request.Body);
            var section = await reader.ReadNextSectionAsync();

            byte[] data = null;
            string signature = null;
            string dataFileName = null;

            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
                if (hasContentDispositionHeader && contentDisposition.DispositionType == "form-data")
                {
                    var name = contentDisposition.Name.Value;

                    using var memoryStream = new MemoryStream();
                    await section.Body.CopyToAsync(memoryStream);

                    if (memoryStream.Length == 0)
                        throw new ExtendedException($"The request couldn't be processed ({name} section has no data)");

                    if (memoryStream.Length > Constants.MaxMultipartRequestSize)
                        throw new ExtendedException($"The request couldn't be processed ({name} section has too many data)");

                    switch (name.ToLowerInvariant())
                    {
                        case "signature":
                            signature = Encoding.UTF8.GetString(memoryStream.ToArray());
                            break;

                        case "data":
                            data = memoryStream.ToArray();
                            dataFileName = contentDisposition.FileName.Value;
                            break;
                    }
                }

                section = await reader.ReadNextSectionAsync();
            }

            if (!(data?.Length > 0))
                throw new ExtendedException($"The request couldn't be processed - no data section found");

            if (!(signature?.Length > 0))
                throw new ExtendedException($"The request couldn't be processed - no signature section found");

            if (!(dataFileName?.Length > 0))
                throw new ExtendedException($"The request couldn't be processed - no file name found");

            var actualSignature = GetHMACSHA1Hash(data, dataFileName);
            if (actualSignature != signature)
                throw new ExtendedException($"The request couldn't be processed - bad signature");

            return data;

        }

        private static string GetHMACSHA1Hash(byte[] inputBytes, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            using var ms = new MemoryStream(inputBytes);
            using var hmac = new HMACSHA1(keyBytes);

            var hash = hmac.ComputeHash(ms);
            return Convert.ToBase64String(hash);
        }

        // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
        // The spec at https://tools.ietf.org/html/rfc2046#section-5.1 states that 70 characters is a reasonable limit.
        private static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

            if (string.IsNullOrWhiteSpace(boundary))
                throw new ExtendedException("Missing content-type boundary.");

            if (boundary.Length > lengthLimit)
                throw new InvalidDataException($"Multipart boundary exceeded length limit {lengthLimit}.");

            return boundary;
        }

    }
}
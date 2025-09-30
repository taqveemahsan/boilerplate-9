using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AuditPilot.API.Swagger
{
    public class FiscalPeriodExamplesFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var path = context.ApiDescription.RelativePath?.ToLowerInvariant() ?? string.Empty;
            if (!path.StartsWith("api/fiscalperiods")) return;

            var method = context.ApiDescription.HttpMethod?.ToUpperInvariant();

            if (method == "GET" && path == "api/fiscalperiods")
            {
                var example = """
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 12,
        "clientId": "18f4a8de-9ad0-4a2d-8a98-144e7c3a9049",
        "name": "FY 2024",
        "startDate": "2024-01-01",
        "endDate": "2024-12-31",
        "isLocked": false,
        "createdAtUtc": "2024-01-02T07:32:11Z"
      },
      {
        "id": 13,
        "clientId": "18f4a8de-9ad0-4a2d-8a98-144e7c3a9049",
        "name": "FY 2025",
        "startDate": "2025-01-01",
        "endDate": "2025-12-31",
        "isLocked": true,
        "createdAtUtc": "2025-01-02T08:00:00Z"
      }
    ],
    "total": 2,
    "page": 1,
    "pageSize": 25
  },
  "errors": []
}
""";
                SetResponseExample(operation, "200", example);
            }

            if (method == "GET" && path == "api/fiscalperiods/{id}")
            {
                var example = """
{
  "success": true,
  "data": {
    "id": 12,
    "clientId": "18f4a8de-9ad0-4a2d-8a98-144e7c3a9049",
    "name": "FY 2024",
    "startDate": "2024-01-01",
    "endDate": "2024-12-31",
    "isLocked": false,
    "createdAtUtc": "2024-01-02T07:32:11Z"
  },
  "errors": []
}
""";
                SetResponseExample(operation, "200", example);
            }

            if (method == "POST" && path == "api/fiscalperiods")
            {
                const string requestExample = """
{
  "clientId": "18f4a8de-9ad0-4a2d-8a98-144e7c3a9049",
  "name": "FY 2026",
  "startDate": "2026-01-01",
  "endDate": "2026-12-31"
}
""";
                SetRequestExample(operation, requestExample);

                var responseExample = """
{
  "success": true,
  "data": {
    "id": 14,
    "clientId": "18f4a8de-9ad0-4a2d-8a98-144e7c3a9049",
    "name": "FY 2026",
    "startDate": "2026-01-01",
    "endDate": "2026-12-31",
    "isLocked": false,
    "createdAtUtc": "2026-01-02T09:12:34Z"
  },
  "errors": []
}
""";
                SetResponseExample(operation, "201", responseExample);
            }

            if (method == "POST" && path.StartsWith("api/fiscalperiods/update/"))
            {
                const string requestExample = """
{
  "clientId": "18f4a8de-9ad0-4a2d-8a98-144e7c3a9049",
  "name": "FY 2024 - Restated",
  "startDate": "2024-01-01",
  "endDate": "2024-12-31"
}
""";
                SetRequestExample(operation, requestExample);

                var responseExample = """
{
  "success": true,
  "data": {
    "id": 12,
    "clientId": "18f4a8de-9ad0-4a2d-8a98-144e7c3a9049",
    "name": "FY 2024 - Restated",
    "startDate": "2024-01-01",
    "endDate": "2024-12-31",
    "isLocked": false,
    "createdAtUtc": "2024-01-02T07:32:11Z"
  },
  "errors": []
}
""";
                SetResponseExample(operation, "200", responseExample);
            }

            if (method == "POST" && path.StartsWith("api/fiscalperiods/delete/"))
            {
                var responseExample = """
{
  "success": true,
  "data": null,
  "errors": []
}
""";
                SetResponseExample(operation, "200", responseExample);
            }

            if (method == "POST" && path.StartsWith("api/fiscalperiods/lock/"))
            {
                var responseExample = """
{
  "success": true,
  "data": {
    "id": 12,
    "clientId": "18f4a8de-9ad0-4a2d-8a98-144e7c3a9049",
    "name": "FY 2024",
    "startDate": "2024-01-01",
    "endDate": "2024-12-31",
    "isLocked": true,
    "createdAtUtc": "2024-01-02T07:32:11Z"
  },
  "errors": []
}
""";
                SetResponseExample(operation, "200", responseExample);
            }
        }

        private static void SetResponseExample(OpenApiOperation operation, string statusCode, string json)
        {
            if (!operation.Responses.TryGetValue(statusCode, out var response)) return;

            foreach (var content in response.Content.Values)
            {
                content.Example = new OpenApiString(json);
            }
        }

        private static void SetRequestExample(OpenApiOperation operation, string json)
        {
            if (operation.RequestBody == null) return;

            foreach (var kvp in operation.RequestBody.Content)
            {
                kvp.Value.Example = new OpenApiString(json);
            }
        }
    }
}

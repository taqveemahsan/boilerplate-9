using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AuditPilot.API.Swagger
{
    public class AdjustmentExamplesFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var path = context.ApiDescription.RelativePath?.ToLowerInvariant() ?? string.Empty;
            if (!path.StartsWith("api/adjustments"))
            {
                return;
            }

            var method = context.ApiDescription.HttpMethod?.ToUpperInvariant();

            if (method == "GET" && path == "api/adjustments")
            {
                var responseExample = """
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 27,
        "fiscalPeriodId": 5,
        "description": "Revenue accrual true-up",
        "postedAtUtc": "2025-02-05T12:22:09Z",
        "createdAtUtc": "2025-02-05T12:22:09Z",
        "updatedAtUtc": "2025-02-05T12:22:09Z",
        "lines": [
          {
            "id": 98,
            "accountId": "9c2b0bdc-27a3-4e0f-9c9f-2a0509f3c7d4",
            "accountCode": "4000",
            "accountName": "Consulting Revenue",
            "debit": 2500.00,
            "credit": 0.00,
            "lineMemo": "Adjust recognition"
          },
          {
            "id": 99,
            "accountId": "4a57f503-1ac5-49a2-8e20-2a5f2d6f7e51",
            "accountCode": "4800",
            "accountName": "Deferred Revenue",
            "debit": 0.00,
            "credit": 2500.00,
            "lineMemo": "Balance adjustment"
          }
        ]
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 50
  },
  "errors": []
}
""";
                SetResponseExample(operation, "200", responseExample);
            }

            if (method == "GET" && path == "api/adjustments/{id}")
            {
                var responseExample = """
{
  "success": true,
  "data": {
    "id": 27,
    "fiscalPeriodId": 5,
    "description": "Revenue accrual true-up",
    "postedAtUtc": "2025-02-05T12:22:09Z",
    "createdAtUtc": "2025-02-05T12:22:09Z",
    "updatedAtUtc": "2025-02-05T12:22:09Z",
    "lines": [
      {
        "id": 98,
        "accountId": "9c2b0bdc-27a3-4e0f-9c9f-2a0509f3c7d4",
        "accountCode": "4000",
        "accountName": "Consulting Revenue",
        "debit": 2500.00,
        "credit": 0.00,
        "lineMemo": "Adjust recognition"
      },
      {
        "id": 99,
        "accountId": "4a57f503-1ac5-49a2-8e20-2a5f2d6f7e51",
        "accountCode": "4800",
        "accountName": "Deferred Revenue",
        "debit": 0.00,
        "credit": 2500.00,
        "lineMemo": "Balance adjustment"
      }
    ]
  },
  "errors": []
}
""";
                SetResponseExample(operation, "200", responseExample);
            }

            if (method == "POST" && path == "api/adjustments")
            {
                const string requestExample = """
{
  "fiscalPeriodId": 5,
  "description": "Revenue accrual true-up",
  "lines": [
    {
      "accountCode": "4000",
      "debit": 2500.00,
      "credit": 0.00,
      "lineMemo": "Adjust recognition"
    },
    {
      "accountCode": "4800",
      "debit": 0.00,
      "credit": 2500.00,
      "lineMemo": "Balance adjustment"
    }
  ]
}
""";
                SetRequestExample(operation, requestExample);

                var responseExample = """
{
  "success": true,
  "data": {
    "id": 27,
    "fiscalPeriodId": 5,
    "description": "Revenue accrual true-up",
    "postedAtUtc": "2025-02-05T12:22:09Z",
    "createdAtUtc": "2025-02-05T12:22:09Z",
    "updatedAtUtc": "2025-02-05T12:22:09Z",
    "lines": [
      {
        "id": 98,
        "accountId": "9c2b0bdc-27a3-4e0f-9c9f-2a0509f3c7d4",
        "accountCode": "4000",
        "accountName": "Consulting Revenue",
        "debit": 2500.00,
        "credit": 0.00,
        "lineMemo": "Adjust recognition"
      },
      {
        "id": 99,
        "accountId": "4a57f503-1ac5-49a2-8e20-2a5f2d6f7e51",
        "accountCode": "4800",
        "accountName": "Deferred Revenue",
        "debit": 0.00,
        "credit": 2500.00,
        "lineMemo": "Balance adjustment"
      }
    ]
  },
  "errors": []
}
""";
                SetResponseExample(operation, "201", responseExample);
            }

            if (method == "POST" && path.StartsWith("api/adjustments/update/"))
            {
                const string requestExample = """
{
  "fiscalPeriodId": 5,
  "description": "Revenue accrual reclass",
  "lines": [
    {
      "accountCode": "4000",
      "debit": 1800.00,
      "credit": 0.00,
      "lineMemo": "Adjust recognition"
    },
    {
      "accountCode": "4800",
      "debit": 0.00,
      "credit": 1800.00,
      "lineMemo": "Balance adjustment"
    }
  ]
}
""";
                SetRequestExample(operation, requestExample);

                var responseExample = """
{
  "success": true,
  "data": {
    "id": 27,
    "fiscalPeriodId": 5,
    "description": "Revenue accrual reclass",
    "postedAtUtc": "2025-02-10T08:12:41Z",
    "createdAtUtc": "2025-02-05T12:22:09Z",
    "updatedAtUtc": "2025-02-10T08:12:41Z",
    "lines": [
      {
        "id": 101,
        "accountId": "9c2b0bdc-27a3-4e0f-9c9f-2a0509f3c7d4",
        "accountCode": "4000",
        "accountName": "Consulting Revenue",
        "debit": 1800.00,
        "credit": 0.00,
        "lineMemo": "Adjust recognition"
      },
      {
        "id": 102,
        "accountId": "4a57f503-1ac5-49a2-8e20-2a5f2d6f7e51",
        "accountCode": "4800",
        "accountName": "Deferred Revenue",
        "debit": 0.00,
        "credit": 1800.00,
        "lineMemo": "Balance adjustment"
      }
    ]
  },
  "errors": []
}
""";
                SetResponseExample(operation, "200", responseExample);
            }

            if (method == "POST" && path.StartsWith("api/adjustments/delete/"))
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
        }

        private static void SetResponseExample(OpenApiOperation operation, string statusCode, string json)
        {
            if (!operation.Responses.TryGetValue(statusCode, out var response))
            {
                return;
            }

            foreach (var content in response.Content.Values)
            {
                content.Example = new OpenApiString(json);
            }
        }

        private static void SetRequestExample(OpenApiOperation operation, string json)
        {
            if (operation.RequestBody == null)
            {
                return;
            }

            foreach (var kvp in operation.RequestBody.Content)
            {
                kvp.Value.Example = new OpenApiString(json);
            }
        }
    }
}

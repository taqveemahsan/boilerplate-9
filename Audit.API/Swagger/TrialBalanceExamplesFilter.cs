using System.Text;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AuditPilot.API.Swagger
{
    public class TrialBalanceExamplesFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var path = context.ApiDescription.RelativePath?.ToLowerInvariant() ?? string.Empty;
            if (path.StartsWith("api/trialbalance") == false) return;

            // GET list example response
            if (context.ApiDescription.HttpMethod?.Equals("GET", StringComparison.OrdinalIgnoreCase) == true && path == "api/trialbalance")
            {
                var exampleJson = """
{
  "items": [
    {
      "id": 1,
      "fiscalPeriodId": 1,
      "accountCode": "10140",
      "accountName": "Cash",
      "cy_Debit": 515.00,
      "cy_Credit": 0.00,
      "adj_Debit": 0.00,
      "adj_Credit": 0.00,
      "cy_Adjusted_Debit": 515.00,
      "cy_Adjusted_Credit": 0.00,
      "py_Debit": 0.00,
      "py_Credit": 0.00,
      "notes": "Main vault"
    },
    {
      "id": 2,
      "fiscalPeriodId": 1,
      "accountCode": "10150",
      "accountName": "Bank",
      "cy_Debit": 16.00,
      "cy_Credit": 26.00,
      "adj_Debit": 0.00,
      "adj_Credit": 0.00,
      "cy_Adjusted_Debit": 16.00,
      "cy_Adjusted_Credit": 26.00,
      "py_Debit": 0.00,
      "py_Credit": 0.00,
      "notes": "Checking"
    }
  ],
  "total": 2,
  "page": 1,
  "pageSize": 50
}
""";
                AddResponseExample(operation, "200", exampleJson);
            }

            // PUT update example request
            if (context.ApiDescription.HttpMethod?.Equals("PUT", StringComparison.OrdinalIgnoreCase) == true && path.StartsWith("api/trialbalance/{id}"))
            {
                var ex = """
{
  "cy_Debit": 520,
  "cy_Credit": 0,
  "adj_Debit": 0,
  "adj_Credit": 0,
  "py_Debit": 0,
  "py_Credit": 0,
  "notes": "final",
  "rowVersion": "AAAAAAAAB9E="
}
""";
                AddRequestExample(operation, ex, "application/json");
            }

            // POST import example
            if (context.ApiDescription.HttpMethod?.Equals("POST", StringComparison.OrdinalIgnoreCase) == true && path == "api/trialbalance/import")
            {
                var csv = """
Code,Name,Level1,Level2,Level3,Level4,CY_Debit,CY_Credit,Adj_Debit,Adj_Credit,PY_Debit,PY_Credit,Notes
10140,Cash,Current Assets,Cash & Equivalents,,,515,0,0,0,0,0,Main vault
10150,Bank,Current Assets,Cash & Equivalents,,,16,26,0,0,0,0,Checking
""";
                var resp = """
{
  "jobId": "a1b2c3d4-...-e9",
  "status": "Completed",
  "inserted": 2,
  "updated": 0,
  "warnings": ["Adjusted totals not balanced: 531 vs 26"],
  "errors": []
}
""";
                AddResponseExample(operation, "202", resp);
                // Represent CSV sample as a string example in text/plain
                AddRequestExample(operation, csv, "text/plain");
            }
        }

        private static void AddResponseExample(OpenApiOperation op, string statusCode, string json)
        {
            if (op.Responses.TryGetValue(statusCode, out var resp))
            {
                foreach (var media in resp.Content.Values)
                {
                    media.Example = new OpenApiString(json);
                }
            }
        }

        private static void AddRequestExample(OpenApiOperation op, string content, string mime)
        {
            if (op.RequestBody == null) return;
            foreach (var kvp in op.RequestBody.Content)
            {
                kvp.Value.Example = new OpenApiString(content);
            }
        }
    }
}

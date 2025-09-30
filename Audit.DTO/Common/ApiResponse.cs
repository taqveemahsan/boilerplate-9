namespace AuthPilot.Models.Common
{
    public record ApiResponse<T>(bool Success, T? Data, IReadOnlyList<string> Errors)
    {
        public static ApiResponse<T> Ok(T data) => new(true, data, Array.Empty<string>());

        public static ApiResponse<T> Fail(params string[] errors) =>
            new(false, default, errors?.Length > 0 ? errors : Array.Empty<string>());

        public static ApiResponse<T> Fail(IEnumerable<string> errors) =>
            new(false, default, errors?.ToArray() ?? Array.Empty<string>());
    }

    public static class ApiResponse
    {
        public static ApiResponse<T> Ok<T>(T data) => ApiResponse<T>.Ok(data);

        public static ApiResponse<T> Fail<T>(params string[] errors) => ApiResponse<T>.Fail(errors);

        public static ApiResponse<T> Fail<T>(IEnumerable<string> errors) => ApiResponse<T>.Fail(errors);
    }
}

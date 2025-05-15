namespace Shipping.Shared;

public class ApiResponse
{
    public bool IsSuccess { get; private set; }
    public List<Error>? Errors { get; private set; } 
    
    public static ApiResponse Success()
    {
        return new ApiResponse
        {
            IsSuccess = true
        };
    }
    
    public static ApiResponse<T> Success<T>(T data)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Data = data
        };
    }
    public static ApiResponse Failure(string errorCode, string errorMessage)
    {
        return new ApiResponse
        {
            IsSuccess = false,
            Errors = [new Error(errorCode, errorMessage)]
        };
    }
}

public record Error(string ErrorCode, string ErrorMessage);
public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }
}
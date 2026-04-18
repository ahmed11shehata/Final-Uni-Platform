namespace AYA_UIS.Shared.Exceptions
{
    /// <summary>
    /// Exception for 422 Unprocessable Entity — business-rule violations
    /// </summary>
    public class UnprocessableEntityException : BaseException
    {
        public UnprocessableEntityException(string message, string errorCode = "UNPROCESSABLE_ENTITY")
            : base(message, errorCode, 422)
        {
        }
    }
}

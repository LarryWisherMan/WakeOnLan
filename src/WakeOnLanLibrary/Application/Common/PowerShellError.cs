namespace WakeOnLanLibrary.Application.Common
{
    public class PowerShellError
    {
        public string Message { get; }
        public string ErrorId { get; }
        public string TargetObject { get; }

        public PowerShellError(string message, string errorId, string targetObject)
        {
            Message = message;
            ErrorId = errorId;
            TargetObject = targetObject;
        }
    }

}

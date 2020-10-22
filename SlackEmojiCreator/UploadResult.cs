namespace SlackEmojiCreator.Upload
{
    public sealed class UploadResult
    {
        public readonly bool IsSucceeded;
        public readonly FailureReason? FailureReason;

        public UploadResult(bool isSucceeded, FailureReason failureReason)
        {
            this.IsSucceeded = isSucceeded;
            this.FailureReason = failureReason;
        }
    }
}

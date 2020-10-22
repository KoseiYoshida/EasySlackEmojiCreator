namespace SlackEmojiCreator.Upload
{
    public enum FailureReason
    {
        //Succeeded
        NotFailed,

        FilepathIsInvalid,
        FileNotExist,
        FileExtensionIsNotAvailable,
        UriIsInvalid,
        RequestError,
        NameIsInvalid,
        NameIsAlreadyExist,
        ImageIsInvalid,
        OverLimit,
        AuthError,
        UploadedFromBot,
        TimeOut,

        // Details of failure is not defined
        NoDetail
    }
}

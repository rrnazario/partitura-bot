namespace TelegramPartHook.Domain.Constants;

public class Enums
{
    public enum SendFileResult
    {
        SUCCESS,
        BLOCKED_BY_USER,
        GENERAL_ERROR
    }

    public enum MessageName
    {
        TooSmallTerm,
        UnableSendVIPInfo,
        NotVipUser,
        MonitorForMe,
        ReminderDefined,
        RememberNumberExceeded,
        FailedToDefineRemember,
        RememberFound,
        SeeYou,
        Unsubscribed,
        BeginningConversion,
        VideoDownloaded,
        TooLongVideo,
        CannotDownloadVideo,
        SearchingMessage1,
        SearchingMessage2,
        SearchingMessage3,
        FollowUsInstagram,
        NotFoundMessage1,
        NotFoundMessage2,
        NotFoundMessage3,
        NotFoundMessage4,
        SuccessMessage1,
        SuccessMessage2,
        SuccessMessage3,
        SuccessMessage4,
        SuccessMessage5,
        AbortedSuccessfully,
        SearchingSuggestion,
        WarningManyMessages,
        WeAreUnderMaintenance,
        ChoseRemember,
        ConfirmRememberExclusion,
        OperationFinalized,
        ThereAreNoRemembers,
        RememberSuccessfullyRemoved,            
        DownloadAsPDF,
        AddRepertoireViaTelegram,
        MakeMeVIP,
        SendAdminMessage,
        MessageSuccessfullySentToAdmin,
        SendSuggestion,            
        SeeMyVIPBenefits,
        WelcomeToVip,
        Contribute,
        Report,
        ReportedSuccessfully,
    }

    public enum FileSource
    {
        Crawler,
        Dropbox,
        Instagram,
        Generated,
        CrawlerDownloadLink,
    }

    public enum CommandAbortReason
    {
        Default,
        TooSmallSearch,
        AbortedByUser
    }
}
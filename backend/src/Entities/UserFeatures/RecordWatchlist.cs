namespace UKPS.Api.Entities.UserFeatures;

internal sealed class RecordWatchlist
{
    public int UserId { get; set; }
    public int RecordId { get; set; }

    // Navigation
    public Identity.User? User { get; set; }
    public RecordWorkflow.Record? Record { get; set; }
}

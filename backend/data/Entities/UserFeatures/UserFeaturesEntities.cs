namespace UKPS.Data.Entities.UserFeatures;

public class RecordWatchlist
{
    public int UserId { get; set; }
    public int RecordId { get; set; }

    // Navigation
    public Identity.User User { get; set; } = null!;
    public RecordWorkflow.Record Record { get; set; } = null!;
}

using CanvasChess;

public static class PlayerSettingsChess
{
    public static PieceColor PlayerColor = PieceColor.White; // Default to White
    
    // Timer settings
    public enum TimerType
    {
        Rapid,   // 10 minutes
        Blitz,   // 5 minutes  
        Bullet    // 3 minutes
    }
    
    public static TimerType SelectedTimerType = TimerType.Rapid; // Default to Rapid
    
    // Get timer duration in seconds based on selected type
    public static float GetTimerDuration()
    {
        return SelectedTimerType switch
        {
            TimerType.Rapid => 600f,  // 10 minutes
            TimerType.Blitz => 300f,  // 5 minutes
            TimerType.Bullet => 180f, // 3 minutes
            _ => 600f // Default to Rapid
        };
    }
} 
namespace BlueSeaGameFramework.server;


public enum MessageId : int
{
    None = 0,
    GameInitEventMsg = 101,
    GameStartEventMsg = 102,
    PlayerTurnChangeEventMsg = 103,
    EndcurrentTurnEventMsg = 104,
    PlayerMoveEventMsg = 107,
    GameTimePauseEventMag = 105,
    GameTimeResumeEventMsg = 106
}
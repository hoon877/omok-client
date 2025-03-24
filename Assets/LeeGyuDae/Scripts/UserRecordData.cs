[System.Serializable]
public class UserRecordData
{
    public int profileIndex;
    public int grade;
    public string nickName;
    public int totalGameCount;
    public int wins;
    public int draws;
    public int losses;
    public float winRate;
    
    public UserRecordData(int profileIndex, int grade, string nickName, int totalGameCount, int wins, int draws, int losses, float winRate)
    {
        this.profileIndex   = profileIndex;
        this.grade          = grade;
        this.nickName       = nickName;
        this.totalGameCount = totalGameCount;
        this.wins           = wins;
        this.draws          = draws; 
        this.losses         = losses;
        this.winRate        = winRate;
    }
}


public class UserRankingData
{
    public int Rank { get; private set; }
    public int ProfileImageIndex { get; private set; }
    public int Grade { get; private set; }
    public string NickName { get; private set; }
    public int TotalGameCount { get; private set; }
    public int Wins { get; private set; }
    public int Losses { get; private set; }

    public UserRankingData(int rank, int profileImageIndex, int grade, string nickName, int totalGameCount, int wins, int losses)
    {
        Rank              = rank;
        ProfileImageIndex = profileImageIndex;
        Grade             = grade;
        NickName          = nickName;
        TotalGameCount    = totalGameCount;
        Wins              = wins;
        Losses            = losses;
    }
}
[System.Serializable]
public class ServerData {

	public bool success;
	public string status;

	public string gamemode;			        // current game mode (circuit)
	public int challengeIndex;			    // current game index
	public int trackTimeTotal;			    // total time across all tracks
	public int totalTracksCompleted;		// total num of tracks completed
	public bool isTracksCompleted;          // determines if user has completed all tracks in circuit

    public bool previouslyCompleted;	    // determines if user has completed all the tracks before

	public int previousTrackTime;		    // the last time made on a sepecific track type

	// Leaderboard Data
	public string userid;
	public string fullname;
	public int total_time;
	public int rank;
	public int regionRank;
	public int orgRank;

    public int currentCircuitTime;
    public int currentTrackRecordTime;
    public int totalAllCircuitsTime;

    public TrackData[] track_data;
    public LeaderboardData[] leaderboard_data;
    public TrackCompletion completion;
    public CircuitRecordTimes circuitRecordTimes;
}

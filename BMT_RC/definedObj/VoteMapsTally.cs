﻿namespace HawkSync_RC.classes
{
    public class VoteMapsTally
    {
        public enum VoteStatus
        {
            VOTE_NO = 0,
            VOTE_YES = 1
        }

        public int Slot { get; set; }
        public string PlayerName { get; set; }
        public VoteStatus Vote { get; set; }
    }
}
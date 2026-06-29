export const TOUR_CONFIG = {

  popularity: {
    /** Tours with fewer than this many logs are rated 'New'. */
    knownMinLogs: 1,
    /** Tours with at least this many logs are rated 'Popular'. */
    popularMinLogs: 3,
  },

  childFriendliness: {
    /**
     * When a tour has NO logs, use estimated distance to guess suitability.
     * Tours at or below this km threshold are 'Likely child-friendly'.
     */
    noLogsMaxDistanceKm: 10,

    /**
     * For tours WITH logs, compare average difficulty (1–5) and average distance (km).
     * Both conditions must be met for that rating to apply.
     */
    friendlyMaxAvgDifficulty: 2,
    friendlyMaxAvgDistanceKm: 12,

    moderateMaxAvgDifficulty: 3,
    moderateMaxAvgDistanceKm: 25,
    // Anything above moderate thresholds → 'Challenging'
  },

} as const;

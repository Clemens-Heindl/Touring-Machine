export const TOUR_CONFIG = {

  popularity: {
    /** Tours with fewer than this many logs are rated 'New'. */
    knownMinLogs: 1,
    /** Tours with at least this many logs are rated 'Popular'. */
    popularMinLogs: 3,
  },

  childFriendliness: {
    /**
     * Tours WITH logs — average across all log entries.
     * ALL three conditions must be satisfied for that tier to apply.
     */
      friendlyMaxAvgDifficulty: 2,    // difficulty scale 1–5
      friendlyMaxAvgDistanceKm: 12,
      friendlyMaxAvgTimeHours: 3,

      moderateMaxAvgDifficulty: 3,
      moderateMaxAvgDistanceKm: 25,
      moderateMaxAvgTimeHours: 6,
      // Anything above moderate → 'Challenging'

    /**
     * Tours WITHOUT logs — derived from the tour's own distance and estimated time.
     * Both conditions must be satisfied for that tier.
     */
      noLogsFriendlyMaxDistanceKm: 10,
      noLogsFriendlyMaxTimeHours: 2,

      noLogsModerateMaxDistanceKm: 25,
      noLogsModerateMaxTimeHours: 5,
      // Anything above → 'Challenging'
  },

} as const;


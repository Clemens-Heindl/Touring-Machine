export interface TransportTypeStat {
  transportType: string;
  tourCount: number;
}

export interface MonthlyActivity {
  month: string;
  logCount: number;
  distanceKm: number;
}

export interface TourSummary {
  id: number;
  name: string;
  detail: string;
}

export interface Statistics {
  tourCount: number;
  logCount: number;
  totalTourDistanceKm: number;
  totalLoggedDistanceKm: number;
  totalLoggedTimeHours: number;
  averageRating: number;
  averageDifficulty: number;
  byTransportType: TransportTypeStat[];
  activityByMonth: MonthlyActivity[];
  mostPopularTour: TourSummary | null;
  mostChallengingTour: TourSummary | null;
}

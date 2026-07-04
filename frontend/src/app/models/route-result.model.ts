export interface ElevationPoint {
  distanceKm: number;
  elevationM: number;
}

export interface RouteResult {
  routeGeoJson: string;
  distanceKm: number;
  estimatedMinutes: number;
  ascentM?: number;
  descentM?: number;
  minElevationM?: number;
  maxElevationM?: number;
  elevationProfile?: ElevationPoint[];
}

export interface TourLog {
    id: number;
    dateTime: string; // Assuming string format from backend
    comment?: string;
    difficulty: number;
    totalDistance: number;
    totalTime: string; // Assuming string format from backend
    rating: number;
    tourId: number;
}

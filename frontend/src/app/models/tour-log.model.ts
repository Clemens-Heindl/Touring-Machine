export interface TourLog {
    id: number;
    dateTime: string;
    comment: string;
    difficulty: number;
    totalDistance: number;
    totalTime: string;
    rating: number;
    tourId: number;
}

export type TourLogFormValue = Omit<TourLog, 'id' | 'tourId'>;

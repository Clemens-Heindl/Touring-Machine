import { TourLog } from './tour-log.model';

export type TransportType = 'Bike' | 'Hike' | 'Running' | 'Vacation';

export interface Tour {
    id: number;
    userId: number;
    name: string;
    description: string;
    from: string;
    to: string;
    transportType: TransportType;
    distance: number;
    estimatedTime: string;
    routeInformation: string;
    imageFileName?: string;
    logs: TourLog[];
    // Computed by the backend business layer (read-only).
    popularity?: string;
    childFriendliness?: string;
}

export type TourFormValue = Omit<Tour, 'id' | 'logs' | 'userId'>;

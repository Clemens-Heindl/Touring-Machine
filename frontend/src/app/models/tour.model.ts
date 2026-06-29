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
    imageUrl: string;
    logs: TourLog[];
}

export type TourFormValue = Omit<Tour, 'id' | 'logs' | 'userId'>;

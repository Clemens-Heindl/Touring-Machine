import { TourLog } from "./tour-log.model";

export interface Tour {
    id: number;
    name: string;
    description?: string;
    from: string;
    to: string;
    transportType: string;
    distance: number;
    estimatedTime: string; // Assuming string format from backend
    routeInformation?: string;
    logs: TourLog[];
}

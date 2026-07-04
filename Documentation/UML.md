# UML

## Class diagram (domain model)

```plantuml
@startuml

class User {
  id : int
  name : string
  email : string
  passwordHash : string  ' BCrypt, server-side
}

class Tour {
  id : int
  name : string
  description : string
  from : string
  to : string
  transportType : string
  distance : double
  estimatedTime : TimeSpan
  routeInformation : string  ' GeoJSON (incl. elevation)
  imageFileName : string     ' file stored on disk
  userId : int
  --
  {computed} popularity
  {computed} childFriendliness
}

class TourLog {
  id : int
  dateTime : DateTime
  comment : string
  difficulty : int
  totalDistance : double
  totalTime : TimeSpan
  rating : int
  tourId : int
  userId : int
}

User "1" <-- "0..*" Tour  : owns
User "1" <-- "0..*" TourLog : createdBy
Tour "1" <-- "0..*" TourLog : relatedTour

@enduml
```

## Layer / component diagram

```plantuml
@startuml
package "Presentation" {
  [ToursController]
  [TourLogsController]
  [UsersController]
  [StatisticsController]
  [ImagesController]
}
package "Business Layer" {
  [TourService]
  [TourLogService]
  [UserService]
  [StatisticsService]
  [ReportService]
  [RouteService]
  [TourAttributeCalculator]
  [JwtTokenService]
  [ImageStorageService]
}
package "Data Access" {
  [TourRepository]
  [TourLogRepository]
  [UserRepository]
  [TourPlannerDbContext]
}

[ToursController] --> [TourService]
[ToursController] --> [ReportService]
[TourLogsController] --> [TourLogService]
[UsersController] --> [UserService]
[UsersController] --> [JwtTokenService]
[StatisticsController] --> [StatisticsService]
[ImagesController] --> [ImageStorageService]

[TourService] --> [TourRepository]
[TourService] --> [TourAttributeCalculator]
[TourLogService] --> [TourLogRepository]
[TourLogService] --> [TourRepository]
[UserService] --> [UserRepository]
[StatisticsService] --> [TourRepository]
[ReportService] --> [TourService]
[ReportService] --> [StatisticsService]

[TourRepository] --> [TourPlannerDbContext]
[TourLogRepository] --> [TourPlannerDbContext]
[UserRepository] --> [TourPlannerDbContext]
@enduml
```

## Sequence diagram — full-text search

```plantuml
@startuml
actor User
participant "Angular\n(TourList)" as UI
participant "ToursController" as Ctrl
participant "TourService" as Svc
participant "TourAttributeCalculator" as Calc
participant "ITourRepository" as Repo
database "PostgreSQL" as DB

User -> UI : type search term
UI -> Ctrl : GET /api/tours/search?q=term\n(Authorization: Bearer <jwt>)
Ctrl -> Ctrl : userId = User.GetUserId()
Ctrl -> Svc : SearchAsync(userId, q)
Svc -> Repo : GetAllByUserAsync(userId)
Repo -> DB : SELECT tours + logs WHERE userId = ...
DB --> Repo : tours
Repo --> Svc : tours (with logs)
loop for each tour
  Svc -> Calc : GetPopularity(tour) / GetChildFriendliness(tour)
  Calc --> Svc : computed values
  Svc -> Svc : build haystack (fields + logs + computed values)
end
Svc -> Svc : keep tours where every token matches
Svc --> Ctrl : List<TourDto>
Ctrl --> UI : 200 OK (matching tours)
UI -> User : render filtered list
@enduml
```

> The PNG exports (`UML Diagramm.png`, `UseCase Diagramm.png`) can be regenerated from these PlantUML
> sources; the class diagram PNG should be refreshed to include `imageFileName` and `TourLog.userId`.
